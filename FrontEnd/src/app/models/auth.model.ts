export type UserRole = 'cliente' | 'socio';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SessionUser {
  token: string;
  role: UserRole;
  nombreCompleto: string;
  email: string;
  demoMode: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  nombreCompleto: string;
  cedula: string;
  telefono: string;
  domicilio: string;
}
