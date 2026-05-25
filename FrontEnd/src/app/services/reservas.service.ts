import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, map, of } from 'rxjs';

import { ReservaResumen } from '../models/reserva.model';
import { RESERVAS_API_BASE_URL } from './api.config';

const MOCK_RESERVAS: ReservaResumen[] = [
  {
    reservaId: 7001,
    codigoReserva: 'LH-7001',
    alojamientoNombre: 'Casa Nube Andina',
    clienteNombre: 'Camila Vela',
    fechaEntrada: '2026-06-12',
    fechaSalida: '2026-06-15',
    estado: 'Confirmada',
    total: 222,
    moneda: 'USD',
  },
  {
    reservaId: 7002,
    codigoReserva: 'LH-7002',
    alojamientoNombre: 'Hotel Puerto Arena',
    clienteNombre: 'Martin Cedeno',
    fechaEntrada: '2026-06-18',
    fechaSalida: '2026-06-21',
    estado: 'Pendiente de pago',
    total: 354,
    moneda: 'USD',
  },
];

@Injectable({ providedIn: 'root' })
export class ReservasService {
  private readonly http = inject(HttpClient);

  getByCliente(clienteId: number | null | undefined, options?: { demoMode?: boolean }) {
    if (!clienteId) {
      return of<ReservaResumen[]>([]);
    }

    return this.http
      .get<unknown>(`${RESERVAS_API_BASE_URL}/Reservas/resumen/cliente/${clienteId}`)
      .pipe(
        map((response) => this.unwrapCollection(response).map((item) => this.normalizeReservation(item))),
        catchError(() => of(options?.demoMode && clienteId === 1 ? MOCK_RESERVAS : [])),
      );
  }

  getPartnerReservations() {
    return of(MOCK_RESERVAS);
  }

  private unwrapCollection(response: unknown) {
    if (Array.isArray(response)) {
      return response;
    }

    if (!response || typeof response !== 'object') {
      return [];
    }

    const collection = response as { data?: unknown; value?: unknown; items?: unknown };

    if (Array.isArray(collection.data)) {
      return collection.data;
    }

    if (Array.isArray(collection.value)) {
      return collection.value;
    }

    if (Array.isArray(collection.items)) {
      return collection.items;
    }

    return [];
  }

  private normalizeReservation(item: any): ReservaResumen {
    const alojamientoId = item.alojamientoId ?? item.AlojamientoId ?? 0;
    const clienteId = item.clienteId ?? item.ClienteId ?? 0;

    return {
      reservaId: item.reservaId ?? item.ReservaId ?? 0,
      codigoReserva: item.codigoReserva ?? item.CodigoReserva ?? undefined,
      alojamientoNombre:
        item.alojamientoNombre ??
        item.AlojamientoNombre ??
        `Alojamiento #${alojamientoId}`,
      clienteNombre:
        item.clienteNombre ??
        item.ClienteNombre ??
        `Cliente #${clienteId}`,
      fechaEntrada: item.fechaEntrada ?? item.FechaEntrada ?? item.fechaCheckIn ?? item.FechaCheckIn ?? '',
      fechaSalida: item.fechaSalida ?? item.FechaSalida ?? item.fechaCheckOut ?? item.FechaCheckOut ?? '',
      estado: item.estado ?? item.Estado ?? 'Pendiente',
      total: Number(item.total ?? item.Total ?? 0) || 0,
      moneda: item.moneda ?? item.Moneda ?? 'USD',
    };
  }
}
