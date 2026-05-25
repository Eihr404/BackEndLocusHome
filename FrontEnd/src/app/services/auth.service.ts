import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map, tap } from 'rxjs';

import { ApiEnvelope } from '../models/api-response.model';
import { LoginRequest, RegisterRequest, SessionUser, UserRole } from '../models/auth.model';
import { USUARIOS_API_BASE_URL } from './api.config';

const SESSION_KEY = 'alojamiento.session';
const CLIENT_PROFILES_KEY = 'alojamiento.client-profiles';
type StoredClientProfile = { clienteId: number; nombreCompleto: string; email: string };
type LoginPayload = {
  token?: string;
  rol?: string;
  nombreCompleto?: string;
  usuarioId?: number;
  clienteId?: number | null;
};
type EnsureClientProfileRequest = {
  usuarioId: number;
  email: string;
  nombreCompleto: string;
  cedula?: string;
  telefono?: string;
  domicilio?: string;
};
type ClientProfilePayload = {
  clienteId?: number;
  email?: string;
  usuarioId?: number | null;
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly sessionSignal = signal<SessionUser | null>(this.readSession());

  readonly session = computed(() => this.sessionSignal());
  readonly isAuthenticated = computed(() => !!this.sessionSignal());
  readonly role = computed(() => this.sessionSignal()?.role ?? null);

  login(payload: LoginRequest): Observable<SessionUser> {
    return this.http
      .post<ApiEnvelope<LoginPayload> | LoginPayload>(`${USUARIOS_API_BASE_URL}/Auth/login`, payload)
      .pipe(
        map((response): LoginPayload => {
          const envelope = response as ApiEnvelope<LoginPayload>;
          return envelope.data ?? (response as LoginPayload);
        }),
        map((response) => {
          if (!response?.token || !response.rol) {
            throw new Error('Respuesta de login incompleta.');
          }

          const session: SessionUser = {
            token: response.token,
            usuarioId: response.usuarioId,
            role: this.normalizeRole(response.rol),
            nombreCompleto: response.nombreCompleto ?? payload.email,
            email: payload.email.trim().toLowerCase(),
            demoMode: false,
            clienteId: this.resolveClienteId(
              payload.email,
              response.clienteId,
              response.nombreCompleto ?? payload.email,
            ),
          };

          return session;
        }),
        tap((session) => this.persistSession(session)),
      );
  }

  register(payload: RegisterRequest) {
    if (payload.rol === 'socio') {
      return this.http.post(`${USUARIOS_API_BASE_URL}/Usuarios`, {
        email: payload.email,
        password: payload.password,
        nombreCompleto: payload.nombreCompleto,
        rol: 'Socio',
      });
    }

    return this.http
      .post(`${USUARIOS_API_BASE_URL}/Clientes/registrar`, {
        email: payload.email,
        password: payload.password,
        nombreCompleto: payload.nombreCompleto,
        cedula: payload.cedula,
        telefono: payload.telefono,
        domicilio: payload.domicilio,
      })
      .pipe(tap(() => undefined));
  }

  ensureClientProfile(payload: EnsureClientProfileRequest) {
    return this.http
      .post<ApiEnvelope<ClientProfilePayload> | ClientProfilePayload>(
        `${USUARIOS_API_BASE_URL}/Clientes/asegurar-perfil`,
        payload,
      )
      .pipe(
        map((response): ClientProfilePayload => {
          const envelope = response as ApiEnvelope<ClientProfilePayload>;
          return envelope.data ?? (response as ClientProfilePayload);
        }),
        tap((profile) => {
          if (!profile?.clienteId) {
            throw new Error('No se pudo obtener el perfil cliente.');
          }

          const currentSession = this.sessionSignal();
          if (currentSession) {
            this.persistSession({
              ...currentSession,
              clienteId: profile.clienteId,
            });
          }

          this.upsertClientProfile(
            payload.email,
            payload.nombreCompleto,
            profile.clienteId,
          );
        }),
      );
  }

  logout() {
    localStorage.removeItem(SESSION_KEY);
    this.sessionSignal.set(null);
    void this.router.navigateByUrl('/');
  }

  private persistSession(session: SessionUser) {
    const normalizedSession: SessionUser = {
      ...session,
      clienteId: session.clienteId && session.clienteId > 0 ? session.clienteId : null,
    };

    localStorage.setItem(SESSION_KEY, JSON.stringify(normalizedSession));
    this.sessionSignal.set(normalizedSession);
  }

  private readSession(): SessionUser | null {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as SessionUser;
      return {
        ...parsed,
        clienteId: parsed.clienteId && parsed.clienteId > 0 ? parsed.clienteId : null,
      };
    } catch {
      localStorage.removeItem(SESSION_KEY);
      return null;
    }
  }

  private normalizeRole(role: string): UserRole {
    return role.trim().toLowerCase() === 'socio' ? 'socio' : 'cliente';
  }

  private resolveClienteId(email: string, apiClienteId: number | null | undefined, nombreCompleto: string) {
    if (apiClienteId && apiClienteId > 0) {
      this.upsertClientProfile(email, nombreCompleto, apiClienteId);
      return apiClienteId;
    }

    return null;
  }

  private upsertClientProfile(email: string, nombreCompleto: string, forcedClienteId?: number) {
    if (!forcedClienteId || forcedClienteId <= 0) {
      return null;
    }

    const normalizedEmail = email.trim().toLowerCase();
    const profiles = this.readClientProfiles();
    const index = profiles.findIndex((item) => item.email.toLowerCase() === normalizedEmail);

    if (index >= 0) {
      profiles[index] = { clienteId: forcedClienteId, nombreCompleto, email: normalizedEmail };
      localStorage.setItem(CLIENT_PROFILES_KEY, JSON.stringify(profiles));
      return forcedClienteId;
    }

    profiles.push({ clienteId: forcedClienteId, nombreCompleto, email: normalizedEmail });
    localStorage.setItem(CLIENT_PROFILES_KEY, JSON.stringify(profiles));
    return forcedClienteId;
  }

  private readClientProfiles(): StoredClientProfile[] {
    const raw = localStorage.getItem(CLIENT_PROFILES_KEY);
    if (!raw) {
      return [];
    }

    try {
      const parsed = JSON.parse(raw) as StoredClientProfile[];
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      localStorage.removeItem(CLIENT_PROFILES_KEY);
      return [];
    }
  }
}
