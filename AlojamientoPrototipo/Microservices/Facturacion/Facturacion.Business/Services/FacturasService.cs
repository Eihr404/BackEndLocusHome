using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Facturacion.Business.DTOs;
using Facturacion.Business.Exceptions;
using Facturacion.Business.Interfaces;
using Facturacion.Business.Mappers;
using Facturacion.DataManagement.Interfaces;
using Facturacion.DataManagement.Models;
using MassTransit;
using Shared.Kernel.Correlation;
using Shared.Kernel.Events;

namespace Facturacion.Business.Services;

public class FacturasService : IFacturasService
{
    private const string CreateOperationName = "CrearFactura";
    private readonly IFacturasDataService _facturasDataService;
    private readonly IIdempotentRequestDataService _idempotentRequestDataService;
    private readonly IAuditoriaDataService _auditoriaDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly CorrelationContextAccessor _correlationAccessor;

    public FacturasService(
        IFacturasDataService facturasDataService,
        IIdempotentRequestDataService idempotentRequestDataService,
        IAuditoriaDataService auditoriaDataService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        CorrelationContextAccessor correlationAccessor)
    {
        _facturasDataService = facturasDataService;
        _idempotentRequestDataService = idempotentRequestDataService;
        _auditoriaDataService = auditoriaDataService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _correlationAccessor = correlationAccessor;
    }

    public async Task<FacturaResponse> GetByIdAsync(int id)
    {
        var factura = await _facturasDataService.GetByIdAsync(id);
        if (factura == null) throw new FacturaNotFoundException(id);
        return FacturacionBusinessMapper.ToResponse(factura);
    }

    public async Task<IEnumerable<FacturaResponse>> GetByReservaIdAsync(int reservaId)
    {
        var facturas = await _facturasDataService.GetByReservaIdAsync(reservaId);
        return facturas.Select(FacturacionBusinessMapper.ToResponse);
    }

    public async Task<IEnumerable<FacturaResumenResponse>> GetResumenByReservaIdAsync(int reservaId)
    {
        var facturas = await _facturasDataService.GetByReservaIdAsync(reservaId);
        return facturas.Select(FacturacionBusinessMapper.ToResumenResponse);
    }

    public async Task<FacturaResponse> CrearAsync(CrearFacturaRequest request)
    {
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var requestHash = ComputeRequestHash(request);

        var idempotentResponse = await TryResolveIdempotentResponseAsync(idempotencyKey, requestHash);
        if (idempotentResponse != null)
        {
            return idempotentResponse;
        }

        decimal totalCalculado = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        if (request.Monto != totalCalculado)
        {
            throw new MontoInvalidoException($"El monto total ({request.Monto}) no coincide con la suma de los detalles ({totalCalculado}).");
        }

        var fechaPagoUtc = NormalizeToUtc(request.FechaPago);

        var model = new FacturaDataModel
        {
            ReservaId = request.ReservaId,
            MetodoPagoId = request.MetodoPagoId,
            Monto = request.Monto,
            FechaPago = fechaPagoUtc,
            Estado = fechaPagoUtc.HasValue ? "Pagado" : "Pendiente",
            Detalles = request.Detalles.Select(d => new DetalleFacturaDataModel
            {
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (idempotencyKey != null)
            {
                var createdPending = await _idempotentRequestDataService.TryCreatePendingAsync(new IdempotentRequestDataModel
                {
                    IdempotencyKey = idempotencyKey,
                    OperationName = CreateOperationName,
                    RequestHash = requestHash,
                    Status = "Pending"
                });

                if (!createdPending)
                {
                    return await ResolveConcurrentDuplicateAsync(idempotencyKey, requestHash);
                }
            }

            var created = await _facturasDataService.CreateAsync(model);

            await _auditoriaDataService.RegistrarAccionAsync(new AuditoriaGeneralDataModel
            {
                NombreTabla = "Facturas",
                Operacion = "INSERT",
                RegistroId = created.FacturaId.ToString(),
                DatosNuevos = $"ReservaId: {created.ReservaId}, Monto: {created.Monto}",
                UsuarioAccion = "Sistema_Facturacion",
                Origen = "FacturasService.CrearAsync"
            });

            if (idempotencyKey != null)
            {
                await _idempotentRequestDataService.MarkCompletedAsync(
                    CreateOperationName,
                    idempotencyKey,
                    created.FacturaId);
            }

            await _unitOfWork.CommitTransactionAsync();

            var pagoPendiente = EventFactory.ApplyMetadata(new PagoPendienteEvent
            {
                ReservaId = created.ReservaId,
                FacturaId = created.FacturaId,
                Monto = created.Monto
            }, "Facturacion.API", _correlationAccessor);

            await _publishEndpoint.Publish(pagoPendiente, publishContext =>
            {
                publishContext.Headers.Set(CorrelationConstants.HeaderName, pagoPendiente.CorrelationId);
            });

            return await GetByIdAsync(created.FacturaId);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<FacturaResponse?> TryResolveIdempotentResponseAsync(string? idempotencyKey, string requestHash)
    {
        if (idempotencyKey == null)
        {
            return null;
        }

        var existing = await _idempotentRequestDataService.GetByKeyAsync(CreateOperationName, idempotencyKey);
        if (existing == null)
        {
            return null;
        }

        ValidateRequestHash(existing.RequestHash, requestHash);

        if (string.Equals(existing.Status, "Completed", StringComparison.OrdinalIgnoreCase) && existing.ResourceId.HasValue)
        {
            return await GetByIdAsync(existing.ResourceId.Value);
        }

        throw new DuplicateOperationInProgressException(CreateOperationName);
    }

    private async Task<FacturaResponse> ResolveConcurrentDuplicateAsync(string idempotencyKey, string requestHash)
    {
        var existing = await _idempotentRequestDataService.GetByKeyAsync(CreateOperationName, idempotencyKey)
            ?? throw new DuplicateOperationInProgressException(CreateOperationName);

        ValidateRequestHash(existing.RequestHash, requestHash);

        if (string.Equals(existing.Status, "Completed", StringComparison.OrdinalIgnoreCase) && existing.ResourceId.HasValue)
        {
            return await GetByIdAsync(existing.ResourceId.Value);
        }

        throw new DuplicateOperationInProgressException(CreateOperationName);
    }

    private static void ValidateRequestHash(string existingHash, string incomingHash)
    {
        if (!string.Equals(existingHash, incomingHash, StringComparison.Ordinal))
        {
            throw new IdempotencyKeyReuseException(CreateOperationName);
        }
    }

    private static string? NormalizeIdempotencyKey(string? idempotencyKey)
        => string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();

    private static string ComputeRequestHash(CrearFacturaRequest request)
    {
        var canonicalPayload = JsonSerializer.Serialize(new
        {
            request.ReservaId,
            request.MetodoPagoId,
            request.Monto,
            FechaPago = NormalizeToUtc(request.FechaPago),
            Detalles = request.Detalles
                .Select(x => new { x.Descripcion, x.Cantidad, x.PrecioUnitario })
                .OrderBy(x => x.Descripcion)
                .ThenBy(x => x.Cantidad)
                .ThenBy(x => x.PrecioUnitario)
                .ToList()
        });

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayload));
        return Convert.ToHexString(bytes);
    }

    public async Task AprobarFacturaAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var factura = await _facturasDataService.GetByIdAsync(id)
                ?? throw new FacturaNotFoundException(id);

            await _facturasDataService.UpdateStatusAsync(id, "Pagado");

            await _auditoriaDataService.RegistrarAccionAsync(new AuditoriaGeneralDataModel
            {
                NombreTabla = "Facturas",
                Operacion = "UPDATE",
                RegistroId = id.ToString(),
                DatosNuevos = "Estado -> Pagado",
                UsuarioAccion = "Sistema_Facturacion",
                Origen = "FacturasService.AprobarFacturaAsync"
            });

            await _unitOfWork.CommitTransactionAsync();

            var facturaPagada = EventFactory.ApplyMetadata(new FacturaPagadaEvent
            {
                ReservaId = factura.ReservaId,
                FacturaId = factura.FacturaId,
                MontoPagado = factura.Monto,
                FechaPago = DateTime.UtcNow
            }, "Facturacion.API", _correlationAccessor);

            await _publishEndpoint.Publish(facturaPagada, publishContext =>
            {
                publishContext.Headers.Set(CorrelationConstants.HeaderName, facturaPagada.CorrelationId);
            });

            var pagoAprobado = EventFactory.ApplyMetadata(new PagoAprobadoEvent
            {
                ReservaId = factura.ReservaId,
                FacturaId = factura.FacturaId,
                MontoPagado = factura.Monto,
                FechaPago = DateTime.UtcNow
            }, "Facturacion.API", _correlationAccessor);

            await _publishEndpoint.Publish(pagoAprobado, publishContext =>
            {
                publishContext.Headers.Set(CorrelationConstants.HeaderName, pagoAprobado.CorrelationId);
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RechazarFacturaAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var factura = await _facturasDataService.GetByIdAsync(id)
                ?? throw new FacturaNotFoundException(id);

            await _facturasDataService.UpdateStatusAsync(id, "Rechazado");

            await _auditoriaDataService.RegistrarAccionAsync(new AuditoriaGeneralDataModel
            {
                NombreTabla = "Facturas",
                Operacion = "UPDATE",
                RegistroId = id.ToString(),
                DatosNuevos = "Estado -> Rechazado",
                UsuarioAccion = "Sistema_Facturacion",
                Origen = "FacturasService.RechazarFacturaAsync"
            });

            await _unitOfWork.CommitTransactionAsync();

            var pagoRechazado = EventFactory.ApplyMetadata(new PagoRechazadoEvent
            {
                ReservaId = factura.ReservaId,
                FacturaId = factura.FacturaId,
                Motivo = "Factura rechazada"
            }, "Facturacion.API", _correlationAccessor);

            await _publishEndpoint.Publish(pagoRechazado, publishContext =>
            {
                publishContext.Headers.Set(CorrelationConstants.HeaderName, pagoRechazado.CorrelationId);
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (!value.HasValue) return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }
}
