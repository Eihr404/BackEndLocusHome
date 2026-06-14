import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../core/api/graphql_client_provider.dart';
import '../core/api/marketplace_api.dart';
import '../core/models.dart';

class ReservationsScreen extends StatefulWidget {
  const ReservationsScreen({
    super.key,
    required this.clienteId,
  });

  final int clienteId;

  @override
  State<ReservationsScreen> createState() => _ReservationsScreenState();
}

class _ReservationsScreenState extends State<ReservationsScreen> {
  late final MarketplaceApi _api;
  late Future<ClientReservations> _reservationsFuture;
  final NumberFormat _currency = NumberFormat.currency(symbol: '\$', decimalDigits: 2);
  int? _processingFacturaId;

  @override
  void initState() {
    super.initState();
    _api = MarketplaceApi(GraphQLClientProvider.instance.client.value);
    _reservationsFuture = _api.getMarketplaceClienteReservas(widget.clienteId);
  }

  Future<void> _refresh() async {
    setState(() {
      _reservationsFuture = _api.getMarketplaceClienteReservas(widget.clienteId);
    });
    await _reservationsFuture;
  }

  Future<void> _approveInvoice(int facturaId, int reservaId) async {
    setState(() => _processingFacturaId = facturaId);
    try {
      await _api.approveInvoice(facturaId);
      if (!mounted) return;
      await Future<void>.delayed(const Duration(seconds: 2));
      await _refresh();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Pago registrado. Esperando confirmacion de la reserva $reservaId por eventos.')),
      );
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.toString().replaceFirst('Exception: ', ''))),
      );
    } finally {
      if (mounted) {
        setState(() => _processingFacturaId = null);
      }
    }
  }

  Future<void> _rejectInvoice(int facturaId, int reservaId) async {
    setState(() => _processingFacturaId = facturaId);
    try {
      await _api.rejectInvoice(facturaId);
      if (!mounted) return;
      await Future<void>.delayed(const Duration(seconds: 2));
      await _refresh();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Cancelacion solicitada. Esperando compensacion de la reserva $reservaId por eventos.')),
      );
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.toString().replaceFirst('Exception: ', ''))),
      );
    } finally {
      if (mounted) {
        setState(() => _processingFacturaId = null);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Mis reservas')),
      body: RefreshIndicator(
        onRefresh: _refresh,
        child: FutureBuilder<ClientReservations>(
          future: _reservationsFuture,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              return ListView(
                children: [
                  const SizedBox(height: 120),
                  Center(
                    child: Padding(
                      padding: const EdgeInsets.all(24),
                      child: Text(snapshot.error.toString()),
                    ),
                  ),
                ],
              );
            }

            final reservations = snapshot.data?.reservas ?? const [];
            if (reservations.isEmpty) {
              return ListView(
                children: const [
                  SizedBox(height: 120),
                  Center(child: Text('Todavia no tienes reservas hechas como cliente.')),
                ],
              );
            }

            return ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: reservations.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final reservation = reservations[index];
                final invoice = reservation.factura;
                final canManagePendingInvoice = invoice != null &&
                    invoice.facturaId == _processingFacturaId
                    ? false
                    : invoice != null &&
                        invoice.estado.toLowerCase() == 'pendiente' &&
                        !reservation.estado.toLowerCase().startsWith('cancelada');

                return Card(
                  child: Padding(
                    padding: const EdgeInsets.all(18),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          reservation.alojamientoNombre,
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.w700,
                              ),
                        ),
                        const SizedBox(height: 8),
                        Text('Codigo: ${reservation.codigoReserva}'),
                        Text('Fechas: ${reservation.fechaEntrada} -> ${reservation.fechaSalida}'),
                        Text('Estado reserva: ${reservation.estado}'),
                        const SizedBox(height: 8),
                        Text(
                          _currency.format(reservation.total),
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.w700,
                              ),
                        ),
                        if (invoice != null) ...[
                          const Divider(height: 24),
                          Text('Factura #${invoice.facturaId}'),
                          Text('Metodo: ${invoice.metodoPagoTipo ?? 'Sin metodo'}'),
                          Text('Estado factura: ${invoice.estado}'),
                          if (invoice.facturaId == _processingFacturaId) ...[
                            const SizedBox(height: 12),
                            const LinearProgressIndicator(),
                          ] else if (canManagePendingInvoice) ...[
                            const SizedBox(height: 16),
                            Row(
                              children: [
                                Expanded(
                                  child: FilledButton(
                                    onPressed: () => _approveInvoice(invoice.facturaId, reservation.reservaId),
                                    child: const Text('Pagar'),
                                  ),
                                ),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: OutlinedButton(
                                    onPressed: () => _rejectInvoice(invoice.facturaId, reservation.reservaId),
                                    child: const Text('Cancelar'),
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ],
                      ],
                    ),
                  ),
                );
              },
            );
          },
        ),
      ),
    );
  }
}
