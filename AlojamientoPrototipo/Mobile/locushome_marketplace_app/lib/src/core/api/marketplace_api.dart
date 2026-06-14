import 'package:graphql_flutter/graphql_flutter.dart';

import '../models.dart';

class MarketplaceApi {
  MarketplaceApi(this._client);

  final GraphQLClient _client;

  Future<LoginSession> login({
    required String email,
    required String password,
  }) async {
    final result = await _client.mutate(
      MutationOptions(
        document: gql(r'''
          mutation Login($input: LoginInput!) {
            login(input: $input) {
              token
              rol
              nombreCompleto
              usuarioId
              clienteId
            }
          }
        '''),
        variables: {
          'input': {
            'email': email,
            'password': password,
          },
        },
      ),
    );

    _throwIfNeeded(result);
    return LoginSession.fromJson(result.data!['login'] as Map<String, dynamic>);
  }

  Future<List<LodgingCard>> getMarketplaceCatalog({
    String? ubicacion,
    DateTime? fechaCheckIn,
    DateTime? fechaCheckOut,
    int? numAdultos,
    int? numNinos,
    bool? admiteMascotas,
    bool? tienePiscina,
    bool? tieneParqueadero,
  }) async {
    final result = await _client.query(
      QueryOptions(
        document: gql(r'''
          query Catalog(
            $ubicacion: String
            $fechaCheckIn: Date
            $fechaCheckOut: Date
            $numAdultos: Int
            $numNinos: Int
            $admiteMascotas: Boolean
            $tienePiscina: Boolean
            $tieneParqueadero: Boolean
          ) {
            marketplaceCatalog(
              ubicacion: $ubicacion
              fechaCheckIn: $fechaCheckIn
              fechaCheckOut: $fechaCheckOut
              numAdultos: $numAdultos
              numNinos: $numNinos
              admiteMascotas: $admiteMascotas
              tienePiscina: $tienePiscina
              tieneParqueadero: $tieneParqueadero
            ) {
              alojamientoId
              nombre
              tipoAlojamiento
              ciudad
              direccion
              precioNocheMinimo
              moneda
              estrellas
              imagenUrl
              admiteMascotas
              tienePiscina
              tieneParqueadero
              disponible
              descripcion
              estado
            }
          }
        '''),
        variables: {
          'ubicacion': _normalizeText(ubicacion),
          'fechaCheckIn': fechaCheckIn == null ? null : _formatDate(fechaCheckIn),
          'fechaCheckOut': fechaCheckOut == null ? null : _formatDate(fechaCheckOut),
          'numAdultos': numAdultos,
          'numNinos': numNinos,
          'admiteMascotas': admiteMascotas,
          'tienePiscina': tienePiscina,
          'tieneParqueadero': tieneParqueadero,
        },
        fetchPolicy: FetchPolicy.networkOnly,
      ),
    );

    _throwIfNeeded(result);
    return (result.data!['marketplaceCatalog'] as List<dynamic>)
        .map((item) => LodgingCard.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<LodgingDetail?> getMarketplaceAlojamientoDetalle(int alojamientoId) async {
    final now = DateTime.now();
    return getMarketplaceAlojamientoDetalleByMonth(
      alojamientoId: alojamientoId,
      mes: now.month,
      anio: now.year,
    );
  }

  Future<LodgingDetail?> getMarketplaceAlojamientoDetalleByMonth({
    required int alojamientoId,
    required int mes,
    required int anio,
  }) async {
    final result = await _client.query(
      QueryOptions(
        document: gql(r'''
          query Detail($alojamientoId: Int!, $mes: Int!, $anio: Int!) {
            marketplaceAlojamientoDetalle(alojamientoId: $alojamientoId) {
              precioNocheMinimo
              precioNocheMaximo
              alojamiento {
                alojamientoId
                tipoAlojamientoNombre
                nombre
                ciudad
                direccion
                descripcion
                estrellas
                calificacionPromedio
                totalResenas
                admiteMascotas
                tienePiscina
                tieneParqueadero
                estado
              }
              habitaciones {
                habitacionId
                nombre
                descripcion
                capacidadAdultos
                capacidadNinos
                numBanos
                numDormitorios
                tieneCocina
                tieneAireAcondicionado
                superficieM2
                precioNoche
                disponibilidadMensual(mes: $mes, anio: $anio) {
                  calendarioId
                  habitacionId
                  fecha
                  estado
                }
              }
              fotos {
                fotoId
                url
                orden
                descripcion
              }
            }
          }
        '''),
        variables: {
          'alojamientoId': alojamientoId,
          'mes': mes,
          'anio': anio,
        },
        fetchPolicy: FetchPolicy.networkOnly,
      ),
    );

    _throwIfNeeded(result);
    final data = result.data!['marketplaceAlojamientoDetalle'];
    if (data == null) {
      return null;
    }

    return LodgingDetail.fromJson(data as Map<String, dynamic>);
  }

  Future<List<AvailabilityEntry>> getRoomAvailability({
    required int habitacionId,
    required int mes,
    required int anio,
  }) async {
    final result = await _client.query(
      QueryOptions(
        document: gql(r'''
          query RoomAvailability($habitacionId: Int!, $mes: Int!, $anio: Int!) {
            marketplaceDisponibilidadHabitacion(habitacionId: $habitacionId, mes: $mes, anio: $anio) {
              calendarioId
              habitacionId
              fecha
              estado
            }
          }
        '''),
        variables: {
          'habitacionId': habitacionId,
          'mes': mes,
          'anio': anio,
        },
        fetchPolicy: FetchPolicy.networkOnly,
      ),
    );

    _throwIfNeeded(result);
    return (result.data!['marketplaceDisponibilidadHabitacion'] as List<dynamic>)
        .map((item) => AvailabilityEntry.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<ClientReservations> getMarketplaceClienteReservas(int clienteId) async {
    final result = await _client.query(
      QueryOptions(
        document: gql(r'''
          query ClientReservations($clienteId: Int!) {
            marketplaceClienteReservas(clienteId: $clienteId) {
              clienteId
              reservas {
                reservaId
                codigoReserva
                alojamientoId
                alojamientoNombre
                clienteNombre
                fechaEntrada
                fechaSalida
                estado
                total
                moneda
                factura {
                  facturaId
                  reservaId
                  metodoPagoTipo
                  monto
                  estado
                  fechaPago
                  fechaCreacion
                  totalDetalles
                }
              }
            }
          }
        '''),
        variables: {'clienteId': clienteId},
        fetchPolicy: FetchPolicy.networkOnly,
      ),
    );

    _throwIfNeeded(result);
    return ClientReservations.fromJson(
      result.data!['marketplaceClienteReservas'] as Map<String, dynamic>,
    );
  }

  Future<List<PaymentMethod>> getMarketplaceMetodosPago() async {
    final result = await _client.query(
      QueryOptions(
        document: gql(r'''
          query PaymentMethods {
            marketplaceMetodosPago {
              metodoPagoId
              tipo
            }
          }
        '''),
        fetchPolicy: FetchPolicy.networkOnly,
      ),
    );

    _throwIfNeeded(result);
    return (result.data!['marketplaceMetodosPago'] as List<dynamic>)
        .map((item) => PaymentMethod.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<Reservation> createReservation({
    required int clienteId,
    required int alojamientoId,
    required DateTime fechaCheckIn,
    required DateTime fechaCheckOut,
    required int numAdultos,
    required int numNinos,
    required bool llevaMascotas,
    required int habitacionId,
    required double precioPorNoche,
    required int numNoches,
    required String idempotencyKey,
  }) async {
    final result = await _client.mutate(
      MutationOptions(
        document: gql(r'''
          mutation CreateReservation($input: CrearReservaInput!) {
            crearReserva(input: $input) {
              reservaId
              estado
              codigoReserva
              total
            }
          }
        '''),
        variables: {
          'input': {
            'clienteId': clienteId,
            'alojamientoId': alojamientoId,
            'fechaCheckIn': _formatDate(fechaCheckIn),
            'fechaCheckOut': _formatDate(fechaCheckOut),
            'numAdultos': numAdultos,
            'numNinos': numNinos,
            'llevaMascotas': llevaMascotas,
            'codigoDescuento': null,
            'idempotencyKey': idempotencyKey,
            'habitaciones': [
              {
                'habitacionId': habitacionId,
                'precioPorNoche': precioPorNoche,
                'numNoches': numNoches,
              }
            ],
          },
        },
      ),
    );

    _throwIfNeeded(result);
    return Reservation.fromJson(result.data!['crearReserva'] as Map<String, dynamic>);
  }

  Future<Invoice> createInvoice({
    required int reservaId,
    required int metodoPagoId,
    required double monto,
    required String descripcion,
    required int cantidad,
    required double precioUnitario,
    required String idempotencyKey,
  }) async {
    final result = await _client.mutate(
      MutationOptions(
        document: gql(r'''
          mutation CreateInvoice($input: CrearFacturaInput!) {
            crearFactura(input: $input) {
              facturaId
              estado
              monto
              reservaId
            }
          }
        '''),
        variables: {
          'input': {
            'reservaId': reservaId,
            'metodoPagoId': metodoPagoId,
            'monto': monto,
            'fechaPago': null,
            'idempotencyKey': idempotencyKey,
            'detalles': [
              {
                'descripcion': descripcion,
                'cantidad': cantidad,
                'precioUnitario': precioUnitario,
              }
            ],
          },
        },
      ),
    );

    _throwIfNeeded(result);
    return Invoice.fromJson(result.data!['crearFactura'] as Map<String, dynamic>);
  }

  Future<void> approveInvoice(int facturaId) async {
    final result = await _client.mutate(
      MutationOptions(
        document: gql(r'''
          mutation ApproveInvoice($facturaId: Int!) {
            aprobarFactura(facturaId: $facturaId) {
              success
              message
            }
          }
        '''),
        variables: {'facturaId': facturaId},
      ),
    );

    _throwIfNeeded(result);
  }

  Future<void> rejectInvoice(int facturaId) async {
    final result = await _client.mutate(
      MutationOptions(
        document: gql(r'''
          mutation RejectInvoice($facturaId: Int!) {
            rechazarFactura(facturaId: $facturaId) {
              success
              message
            }
          }
        '''),
        variables: {'facturaId': facturaId},
      ),
    );

    _throwIfNeeded(result);
  }

  static void _throwIfNeeded(QueryResult result) {
    if (result.hasException) {
      final messages = result.exception!.graphqlErrors
          .map((error) => error.message)
          .join('\n');
      throw Exception(messages.isEmpty ? result.exception.toString() : messages);
    }
  }

  static String _formatDate(DateTime value) {
    final safe = value.toUtc();
    return '${safe.year.toString().padLeft(4, '0')}-${safe.month.toString().padLeft(2, '0')}-${safe.day.toString().padLeft(2, '0')}';
  }

  static String? _normalizeText(String? value) {
    final trimmed = value?.trim();
    return (trimmed == null || trimmed.isEmpty) ? null : trimmed;
  }
}
