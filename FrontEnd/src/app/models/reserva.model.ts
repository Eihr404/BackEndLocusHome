export interface ReservaResumen {
  reservaId: number;
  codigoReserva?: string;
  alojamientoNombre: string;
  clienteNombre: string;
  fechaEntrada: string;
  fechaSalida: string;
  estado: string;
  total: number;
  moneda: string;
}

export interface ReservaHabitacionRequest {
  habitacionId: number;
  precioPorNoche: number;
  numNoches: number;
}

export interface CrearReservaRequest {
  clienteId: number;
  alojamientoId: number;
  fechaCheckIn: string;
  fechaCheckOut: string;
  numAdultos: number;
  numNinos: number;
  llevaMascotas: boolean;
  codigoDescuento?: string;
  habitaciones: ReservaHabitacionRequest[];
}

export interface ReservaCreada {
  reservaId: number;
  codigoReserva?: string;
  estado: string;
  total: number;
  moneda: string;
}
