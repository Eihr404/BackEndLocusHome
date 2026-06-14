class LoginSession {
  LoginSession({
    required this.token,
    required this.rol,
    required this.nombreCompleto,
    required this.usuarioId,
    required this.clienteId,
  });

  final String token;
  final String rol;
  final String nombreCompleto;
  final int usuarioId;
  final int? clienteId;

  factory LoginSession.fromJson(Map<String, dynamic> json) => LoginSession(
        token: json['token'] as String,
        rol: json['rol'] as String,
        nombreCompleto: json['nombreCompleto'] as String,
        usuarioId: json['usuarioId'] as int,
        clienteId: json['clienteId'] as int?,
      );
}

class CatalogSearchFilters {
  CatalogSearchFilters({
    this.ubicacion,
    this.fechaCheckIn,
    this.fechaCheckOut,
    this.numAdultos,
    this.numNinos,
    this.admiteMascotas,
    this.tienePiscina,
    this.tieneParqueadero,
  });

  final String? ubicacion;
  final DateTime? fechaCheckIn;
  final DateTime? fechaCheckOut;
  final int? numAdultos;
  final int? numNinos;
  final bool? admiteMascotas;
  final bool? tienePiscina;
  final bool? tieneParqueadero;
}

class LodgingCard {
  LodgingCard({
    required this.alojamientoId,
    required this.nombre,
    required this.tipoAlojamiento,
    required this.ciudad,
    required this.direccion,
    required this.precioNocheMinimo,
    required this.moneda,
    required this.estrellas,
    required this.imagenUrl,
    required this.admiteMascotas,
    required this.tienePiscina,
    required this.tieneParqueadero,
    required this.disponible,
    required this.descripcion,
    required this.estado,
  });

  final int alojamientoId;
  final String nombre;
  final String tipoAlojamiento;
  final String ciudad;
  final String direccion;
  final double precioNocheMinimo;
  final String moneda;
  final int estrellas;
  final String? imagenUrl;
  final bool admiteMascotas;
  final bool tienePiscina;
  final bool tieneParqueadero;
  final bool disponible;
  final String? descripcion;
  final String estado;

  factory LodgingCard.fromJson(Map<String, dynamic> json) => LodgingCard(
        alojamientoId: json['alojamientoId'] as int,
        nombre: json['nombre'] as String,
        tipoAlojamiento: json['tipoAlojamiento'] as String,
        ciudad: json['ciudad'] as String,
        direccion: json['direccion'] as String,
        precioNocheMinimo: (json['precioNocheMinimo'] as num).toDouble(),
        moneda: json['moneda'] as String,
        estrellas: json['estrellas'] as int,
        imagenUrl: json['imagenUrl'] as String?,
        admiteMascotas: json['admiteMascotas'] as bool,
        tienePiscina: json['tienePiscina'] as bool,
        tieneParqueadero: json['tieneParqueadero'] as bool,
        disponible: json['disponible'] as bool,
        descripcion: json['descripcion'] as String?,
        estado: json['estado'] as String,
      );
}

class Lodging {
  Lodging({
    required this.alojamientoId,
    required this.tipoAlojamientoNombre,
    required this.nombre,
    required this.ciudad,
    required this.direccion,
    required this.descripcion,
    required this.estrellas,
    required this.calificacionPromedio,
    required this.totalResenas,
    required this.admiteMascotas,
    required this.tienePiscina,
    required this.tieneParqueadero,
    required this.estado,
  });

  final int alojamientoId;
  final String tipoAlojamientoNombre;
  final String nombre;
  final String? ciudad;
  final String direccion;
  final String? descripcion;
  final int? estrellas;
  final double calificacionPromedio;
  final int totalResenas;
  final bool admiteMascotas;
  final bool tienePiscina;
  final bool tieneParqueadero;
  final String estado;

  factory Lodging.fromJson(Map<String, dynamic> json) => Lodging(
        alojamientoId: json['alojamientoId'] as int,
        tipoAlojamientoNombre: json['tipoAlojamientoNombre'] as String,
        nombre: json['nombre'] as String,
        ciudad: json['ciudad'] as String?,
        direccion: json['direccion'] as String,
        descripcion: json['descripcion'] as String?,
        estrellas: json['estrellas'] as int?,
        calificacionPromedio: (json['calificacionPromedio'] as num).toDouble(),
        totalResenas: json['totalResenas'] as int,
        admiteMascotas: json['admiteMascotas'] as bool,
        tienePiscina: json['tienePiscina'] as bool,
        tieneParqueadero: json['tieneParqueadero'] as bool,
        estado: json['estado'] as String,
      );
}

class Room {
  Room({
    required this.habitacionId,
    required this.nombre,
    required this.descripcion,
    required this.capacidadAdultos,
    required this.capacidadNinos,
    required this.numBanos,
    required this.numDormitorios,
    required this.tieneCocina,
    required this.tieneAireAcondicionado,
    required this.superficieM2,
    required this.precioNoche,
    required this.disponibilidadMensual,
  });

  final int habitacionId;
  final String nombre;
  final String? descripcion;
  final int capacidadAdultos;
  final int capacidadNinos;
  final int numBanos;
  final int numDormitorios;
  final bool tieneCocina;
  final bool tieneAireAcondicionado;
  final double? superficieM2;
  final double precioNoche;
  final List<AvailabilityEntry> disponibilidadMensual;

  factory Room.fromJson(Map<String, dynamic> json) => Room(
        habitacionId: json['habitacionId'] as int,
        nombre: json['nombre'] as String,
        descripcion: json['descripcion'] as String?,
        capacidadAdultos: json['capacidadAdultos'] as int,
        capacidadNinos: json['capacidadNinos'] as int,
        numBanos: json['numBanos'] as int,
        numDormitorios: json['numDormitorios'] as int,
        tieneCocina: json['tieneCocina'] as bool,
        tieneAireAcondicionado: json['tieneAireAcondicionado'] as bool,
        superficieM2: (json['superficieM2'] as num?)?.toDouble(),
        precioNoche: (json['precioNoche'] as num).toDouble(),
        disponibilidadMensual: ((json['disponibilidadMensual'] as List<dynamic>?) ?? const [])
            .map((item) => AvailabilityEntry.fromJson(item as Map<String, dynamic>))
            .toList(),
      );
}

class AvailabilityEntry {
  AvailabilityEntry({
    required this.calendarioId,
    required this.habitacionId,
    required this.fecha,
    required this.estado,
  });

  final int calendarioId;
  final int habitacionId;
  final DateTime fecha;
  final String estado;

  factory AvailabilityEntry.fromJson(Map<String, dynamic> json) => AvailabilityEntry(
        calendarioId: json['calendarioId'] as int,
        habitacionId: json['habitacionId'] as int,
        fecha: DateTime.parse(json['fecha'] as String),
        estado: json['estado'] as String,
      );
}

class Photo {
  Photo({
    required this.fotoId,
    required this.url,
    required this.orden,
    required this.descripcion,
  });

  final int fotoId;
  final String url;
  final int orden;
  final String? descripcion;

  factory Photo.fromJson(Map<String, dynamic> json) => Photo(
        fotoId: json['fotoId'] as int,
        url: json['url'] as String,
        orden: json['orden'] as int,
        descripcion: json['descripcion'] as String?,
      );
}

class LodgingDetail {
  LodgingDetail({
    required this.alojamiento,
    required this.habitaciones,
    required this.fotos,
    required this.precioNocheMinimo,
    required this.precioNocheMaximo,
  });

  final Lodging alojamiento;
  final List<Room> habitaciones;
  final List<Photo> fotos;
  final double? precioNocheMinimo;
  final double? precioNocheMaximo;

  factory LodgingDetail.fromJson(Map<String, dynamic> json) => LodgingDetail(
        alojamiento: Lodging.fromJson(json['alojamiento'] as Map<String, dynamic>),
        habitaciones: (json['habitaciones'] as List<dynamic>)
            .map((item) => Room.fromJson(item as Map<String, dynamic>))
            .toList(),
        fotos: (json['fotos'] as List<dynamic>)
            .map((item) => Photo.fromJson(item as Map<String, dynamic>))
            .toList(),
        precioNocheMinimo: (json['precioNocheMinimo'] as num?)?.toDouble(),
        precioNocheMaximo: (json['precioNocheMaximo'] as num?)?.toDouble(),
      );
}

class InvoiceSummary {
  InvoiceSummary({
    required this.facturaId,
    required this.reservaId,
    required this.metodoPagoTipo,
    required this.monto,
    required this.estado,
    required this.fechaPago,
    required this.fechaCreacion,
    required this.totalDetalles,
  });

  final int facturaId;
  final int reservaId;
  final String? metodoPagoTipo;
  final double monto;
  final String estado;
  final DateTime? fechaPago;
  final DateTime fechaCreacion;
  final int totalDetalles;

  factory InvoiceSummary.fromJson(Map<String, dynamic> json) => InvoiceSummary(
        facturaId: json['facturaId'] as int,
        reservaId: json['reservaId'] as int,
        metodoPagoTipo: json['metodoPagoTipo'] as String?,
        monto: (json['monto'] as num).toDouble(),
        estado: json['estado'] as String,
        fechaPago: json['fechaPago'] == null
            ? null
            : DateTime.parse(json['fechaPago'] as String),
        fechaCreacion: DateTime.parse(json['fechaCreacion'] as String),
        totalDetalles: json['totalDetalles'] as int,
      );
}

class ReservationItem {
  ReservationItem({
    required this.reservaId,
    required this.codigoReserva,
    required this.alojamientoId,
    required this.alojamientoNombre,
    required this.clienteNombre,
    required this.fechaEntrada,
    required this.fechaSalida,
    required this.estado,
    required this.total,
    required this.moneda,
    required this.factura,
  });

  final int reservaId;
  final String codigoReserva;
  final int alojamientoId;
  final String alojamientoNombre;
  final String clienteNombre;
  final String fechaEntrada;
  final String fechaSalida;
  final String estado;
  final double total;
  final String moneda;
  final InvoiceSummary? factura;

  factory ReservationItem.fromJson(Map<String, dynamic> json) => ReservationItem(
        reservaId: json['reservaId'] as int,
        codigoReserva: json['codigoReserva'] as String,
        alojamientoId: json['alojamientoId'] as int,
        alojamientoNombre: json['alojamientoNombre'] as String,
        clienteNombre: json['clienteNombre'] as String,
        fechaEntrada: json['fechaEntrada'] as String,
        fechaSalida: json['fechaSalida'] as String,
        estado: json['estado'] as String,
        total: (json['total'] as num).toDouble(),
        moneda: json['moneda'] as String,
        factura: json['factura'] == null
            ? null
            : InvoiceSummary.fromJson(json['factura'] as Map<String, dynamic>),
      );
}

class ClientReservations {
  ClientReservations({
    required this.clienteId,
    required this.reservas,
  });

  final int clienteId;
  final List<ReservationItem> reservas;

  factory ClientReservations.fromJson(Map<String, dynamic> json) => ClientReservations(
        clienteId: json['clienteId'] as int,
        reservas: (json['reservas'] as List<dynamic>)
            .map((item) => ReservationItem.fromJson(item as Map<String, dynamic>))
            .toList(),
      );
}

class Reservation {
  Reservation({
    required this.reservaId,
    required this.estado,
    required this.codigoReserva,
    required this.total,
  });

  final int reservaId;
  final String estado;
  final String codigoReserva;
  final double total;

  factory Reservation.fromJson(Map<String, dynamic> json) => Reservation(
        reservaId: json['reservaId'] as int,
        estado: json['estado'] as String,
        codigoReserva: json['codigoReserva'] as String,
        total: (json['total'] as num).toDouble(),
      );
}

class Invoice {
  Invoice({
    required this.facturaId,
    required this.estado,
    required this.monto,
    required this.reservaId,
  });

  final int facturaId;
  final String estado;
  final double monto;
  final int reservaId;

  factory Invoice.fromJson(Map<String, dynamic> json) => Invoice(
        facturaId: json['facturaId'] as int,
        estado: json['estado'] as String,
        monto: (json['monto'] as num).toDouble(),
        reservaId: json['reservaId'] as int,
      );
}

class PaymentMethod {
  PaymentMethod({
    required this.metodoPagoId,
    required this.tipo,
  });

  final int metodoPagoId;
  final String tipo;

  factory PaymentMethod.fromJson(Map<String, dynamic> json) => PaymentMethod(
        metodoPagoId: json['metodoPagoId'] as int,
        tipo: json['tipo'] as String,
      );
}
