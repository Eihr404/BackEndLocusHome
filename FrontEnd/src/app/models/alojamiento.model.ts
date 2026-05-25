export interface AlojamientoCard {
  alojamientoId: number;
  socioId?: number;
  tipoAlojamientoId?: number;
  nombre: string;
  tipoAlojamiento: string;
  ciudad: string;
  direccion: string;
  precioNocheMinimo: number;
  moneda: string;
  estrellas: number;
  imagenUrl?: string;
  admiteMascotas?: boolean;
  tienePiscina?: boolean;
  tieneParqueadero?: boolean;
  disponible?: boolean;
  descripcion?: string;
  estado?: string;
}

export interface Habitacion {
  habitacionId: number;
  alojamientoId: number;
  nombre: string;
  descripcion?: string;
  capacidadAdultos: number;
  capacidadNinos: number;
  numBanos?: number;
  numDormitorios?: number;
  tieneCocina?: boolean;
  tieneAireAcondicionado?: boolean;
  superficieM2?: number | null;
  precioNoche: number;
  estado?: string;
}

export interface FotoAlojamiento {
  fotoId: number;
  alojamientoId: number;
  url: string;
  orden: number;
  descripcion?: string | null;
}

export interface AlojamientoForm {
  socioId: number;
  tipoAlojamientoId: number;
  nombre: string;
  descripcion: string;
  direccion: string;
  ciudad: string;
  estrellas?: number | null;
  admiteMascotas: boolean;
  tienePiscina: boolean;
  tieneParqueadero: boolean;
}

export interface HabitacionForm {
  alojamientoId: number;
  nombre: string;
  descripcion: string;
  capacidadAdultos: number;
  capacidadNinos: number;
  numBanos: number;
  numDormitorios: number;
  tieneCocina: boolean;
  tieneAireAcondicionado: boolean;
  superficieM2?: number | null;
  precioNoche: number;
}

export interface FotoAlojamientoForm {
  alojamientoId: number;
  sourceUrl: string;
  orden: number;
  descripcion: string;
}

export interface TipoAlojamientoOption {
  id: number;
  nombre: string;
}
