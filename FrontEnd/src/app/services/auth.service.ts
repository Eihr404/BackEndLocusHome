import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, of, tap } from 'rxjs';

import { ApiEnvelope } from '../models/api-response.model';
import { LoginRequest, RegisterRequest, SessionUser, UserRole } from '../models/auth.model';
import { USUARIOS_API_BASE_URL } from './api.config';

const SESSION_KEY = 'alojamiento.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly sessionSignal = signal<SessionUser | null>(this.readSession());

  readonly session = computed(() => this.sessionSignal());
  readonly isAuthenticated = computed(() => !!this.sessionSignal());
  readonly role = computed(() => this.sessionSignal()?.role ?? null);

  login(payload: LoginRequest, preferredRole: UserRole) {
    type LoginPayload = { token?: string; rol?: string; nombreCompleto?: string };

    return this.http
      .post<ApiEnvelope<LoginPayload> | LoginPayload>(`${USUARIOS_API_BASE_URL}/Auth/login`, payload)
      .pipe(
        map((response): LoginPayload => {
          const envelope = response as ApiEnvelope<LoginPayload>;
          return envelope.data ?? (response as LoginPayload);
        }),
        tap((response) => {
          if (response?.token) {
            this.persistSession({
              token: response.token,
              role: this.normalizeRole(response.rol, preferredRole),
              nombreCompleto: response.nombreCompleto ?? payload.email,
              email: payload.email,
              demoMode: false,
            });
          } else {
            throw new Error('Login sin token');
          }
        }),
        catchError(() => {
          const demoUsers: Record<UserRole, SessionUser> = {
            cliente: {
              token: 'demo-cliente-token',
              role: 'cliente',
              nombreCompleto: 'Camila Vela',
              email: payload.email,
              demoMode: true,
            },
            socio: {
              token: 'demo-socio-token',
              role: 'socio',
              nombreCompleto: 'Daniel Paredes',
              email: payload.email,
              demoMode: true,
            },
          };

          this.persistSession(demoUsers[preferredRole]);
          return of(demoUsers[preferredRole]);
        }),
      );
  }

  register(payload: RegisterRequest) {
    return this.http.post(`${USUARIOS_API_BASE_URL}/Clientes/registrar`, payload).pipe(
      catchError(() =>
        of({
          mensaje: 'Registro simulado en modo demo',
        }),
      ),
    );
  }

  logout() {
    localStorage.removeItem(SESSION_KEY);
    this.sessionSignal.set(null);
    void this.router.navigateByUrl('/');
  }

  private persistSession(session: SessionUser) {
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
    this.sessionSignal.set(session);
  }

  private readSession(): SessionUser | null {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as SessionUser;
    } catch {
      localStorage.removeItem(SESSION_KEY);
      return null;
    }
  }

  private normalizeRole(role: string | undefined, fallback: UserRole): UserRole {
    const normalized = role?.toLowerCase();
    if (normalized === 'cliente' || normalized === 'socio') {
      return normalized;
    }

    return fallback;
  }
}
