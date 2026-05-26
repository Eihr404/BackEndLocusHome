export interface FacturaResumen {
  facturaId: number;
  reservaId: number;
  estado: string;
  montoTotal: number;
  moneda: string;
  existe?: boolean;
  metodoPagoId?: number | null;
  metodoPagoTipo?: string | null;
  fechaPago?: string | null;
}

export interface MetodoPago {
  metodoPagoId: number;
  nombre: string;
  tipo?: string;
  descripcion?: string;
}

export interface CrearDetalleFacturaRequest {
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
}

export interface CrearFacturaRequest {
  reservaId: number;
  metodoPagoId?: number | null;
  monto: number;
  fechaPago?: string | null;
  detalles: CrearDetalleFacturaRequest[];
}
