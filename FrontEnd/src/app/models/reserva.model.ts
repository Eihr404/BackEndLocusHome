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
