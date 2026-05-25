export type UserRole = 'cliente' | 'socio';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SessionUser {
  token: string;
  usuarioId?: number;
  role: UserRole;
  nombreCompleto: string;
  email: string;
  demoMode: boolean;
  clienteId?: number | null;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nombreCompleto: string;
  cedula: string;
  telefono: string;
  domicilio: string;
  rol?: UserRole;
}
