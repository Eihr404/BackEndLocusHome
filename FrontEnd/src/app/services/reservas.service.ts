import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, forkJoin, map, of } from 'rxjs';

import {
  CrearReservaRequest,
  ReservaAlojamientoDetalle,
  ReservaCreada,
  ReservaDetalleHabitacion,
  ReservaResumen,
} from '../models/reserva.model';
import { API_GATEWAY_BASE_URL, RESERVAS_API_BASE_URL } from './api.config';

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
    console.log('[ReservasService] getByCliente clienteId:', clienteId);
    if (!clienteId) {
      console.warn('[ReservasService] clienteId es null — no se cargan reservas');
      return of<ReservaResumen[]>([]);
    }

    // Llamar directo al microservicio (más confiable que el gateway en Render)
    return this.http
      .get<unknown>(`${RESERVAS_API_BASE_URL}/Reservas/resumen/cliente/${clienteId}`)
      .pipe(
        map((response) => {
          console.log('[ReservasService] respuesta cruda:', JSON.stringify(response));
          const items = this.unwrapCollection(response).map((item) => this.normalizeReservation(item));
          console.log('[ReservasService] normalizadas:', items);
          return items;
        }),
        catchError((err) => {
          console.error('[ReservasService] microservicio falló, intentando gateway:', err.status);
          return this.http
            .get<unknown>(`${API_GATEWAY_BASE_URL}/booking/resumen/cliente/${clienteId}`)
            .pipe(
              map((response) => this.unwrapCollection(response).map((item) => this.normalizeReservation(item))),
              catchError((err2) => {
                console.error('[ReservasService] gateway también falló:', err2.status);
                return of<ReservaResumen[]>([]);
              }),
            );
        }),
      );
  }

  getPartnerReservations() {
    return of(MOCK_RESERVAS);
  }

  getByAlojamiento(alojamientoId: number, alojamientoNombre?: string) {
    return this.http
      .get<unknown>(`${RESERVAS_API_BASE_URL}/Reservas/resumen/alojamiento/${alojamientoId}`)
      .pipe(
        map((response) =>
          this.unwrapCollection(response).map((item) =>
            this.normalizeReservation(item, { alojamientoId, alojamientoNombre }),
          ),
        ),
      );
  }

  getDetailedByAlojamiento(alojamientoId: number) {
    return this.http
      .get<unknown>(`${RESERVAS_API_BASE_URL}/Reservas/alojamiento/${alojamientoId}`)
      .pipe(
        map((response) =>
          this.unwrapCollection(response).map((item) => this.normalizeDetailedReservation(item, alojamientoId)),
        ),
      );
  }

  getByAlojamientos(items: Array<{ alojamientoId: number; nombre?: string }>) {
    if (!items.length) {
      return of<ReservaResumen[]>([]);
    }

    return forkJoin(
      items.map((item) =>
        this.getByAlojamiento(item.alojamientoId, item.nombre).pipe(catchError(() => of<ReservaResumen[]>([]))),
      ),
    ).pipe(
      map((groups) =>
        groups
          .flat()
          .sort((left, right) => {
            const leftDate = Date.parse(left.fechaEntrada || '');
            const rightDate = Date.parse(right.fechaEntrada || '');
            return Number.isNaN(rightDate) || Number.isNaN(leftDate) ? right.reservaId - left.reservaId : rightDate - leftDate;
          }),
      ),
    );
  }

  createReservation(payload: CrearReservaRequest) {
    return this.http
      .post<unknown>(`${API_GATEWAY_BASE_URL}/booking`, payload)
      .pipe(map((response) => this.normalizeCreatedReservation(response)));
  }

  updateReservationStatus(reservaId: number, estado: string) {
    return this.http.patch(`${RESERVAS_API_BASE_URL}/Reservas/${reservaId}/estado`, { estado });
  }

  private unwrapCollection(response: unknown) {
    if (Array.isArray(response)) {
      return response;
    }

    if (!response || typeof response !== 'object') {
      return [];
    }

    const collection = response as {
      data?: unknown;
      Data?: unknown;
      value?: unknown;
      Value?: unknown;
      items?: unknown;
      Items?: unknown;
    };

    if (Array.isArray(collection.data)) {
      return collection.data;
    }

    if (Array.isArray(collection.Data)) {
      return collection.Data;
    }

    if (Array.isArray(collection.value)) {
      return collection.value;
    }

    if (Array.isArray(collection.Value)) {
      return collection.Value;
    }

    if (Array.isArray(collection.items)) {
      return collection.items;
    }

    if (Array.isArray(collection.Items)) {
      return collection.Items;
    }

    return [];
  }

  private normalizeReservation(
    item: any,
    context?: { alojamientoId?: number; alojamientoNombre?: string },
  ): ReservaResumen {
    const alojamientoId = item.alojamientoId ?? item.AlojamientoId ?? context?.alojamientoId ?? 0;
    const clienteId = item.clienteId ?? item.ClienteId ?? 0;

    return {
      reservaId: item.reservaId ?? item.ReservaId ?? 0,
      codigoReserva: item.codigoReserva ?? item.CodigoReserva ?? undefined,
      alojamientoId,
      alojamientoNombre:
        item.alojamientoNombre ??
        item.AlojamientoNombre ??
        context?.alojamientoNombre ??
        `Alojamiento #${alojamientoId}`,
      clienteNombre:
        item.clienteNombre ??
        item.ClienteNombre ??
        `Cliente #${clienteId}`,
      fechaEntrada: item.fechaEntrada ?? item.FechaEntrada ?? item.fechaCheckIn ?? item.FechaCheckIn ?? '',
      fechaSalida: item.fechaSalida ?? item.FechaSalida ?? item.fechaCheckOut ?? item.FechaCheckOut ?? '',
      estado: item.estado ?? item.Estado ?? 'Pendiente',
      total: Number(item.total ?? item.Total ?? item.subTotal ?? item.SubTotal ?? 0) || 0,
      moneda: item.moneda ?? item.Moneda ?? 'USD',
    };
  }

  private normalizeCreatedReservation(response: unknown): ReservaCreada {
    const item = this.unwrapItem(response);

    return {
      reservaId: item?.reservaId ?? item?.ReservaId ?? 0,
      codigoReserva: item?.codigoReserva ?? item?.CodigoReserva ?? undefined,
      estado: item?.estado ?? item?.Estado ?? 'Pendiente',
      total: Number(item?.total ?? item?.Total ?? 0) || 0,
      moneda: item?.moneda ?? item?.Moneda ?? 'USD',
    };
  }

  private normalizeDetailedReservation(item: any, alojamientoId: number): ReservaAlojamientoDetalle {
    const details = item.detallesHabitacion ?? item.DetallesHabitacion ?? [];

    return {
      reservaId: item.reservaId ?? item.ReservaId ?? 0,
      clienteId: item.clienteId ?? item.ClienteId ?? 0,
      alojamientoId: item.alojamientoId ?? item.AlojamientoId ?? alojamientoId,
      fechaCheckIn: item.fechaCheckIn ?? item.FechaCheckIn ?? '',
      fechaCheckOut: item.fechaCheckOut ?? item.FechaCheckOut ?? '',
      estado: item.estado ?? item.Estado ?? 'Pendiente',
      codigoReserva: item.codigoReserva ?? item.CodigoReserva ?? undefined,
      detallesHabitacion: Array.isArray(details)
        ? details.map((detail: any) => this.normalizeReservationDetail(detail))
        : [],
    };
  }

  private normalizeReservationDetail(item: any): ReservaDetalleHabitacion {
    return {
      detalleId: item.detalleId ?? item.DetalleId ?? 0,
      habitacionId: item.habitacionId ?? item.HabitacionId ?? 0,
      precioPorNoche: Number(item.precioPorNoche ?? item.PrecioPorNoche ?? 0) || 0,
      numNoches: item.numNoches ?? item.NumNoches ?? 0,
      subTotalHabitacion: Number(item.subTotalHabitacion ?? item.SubTotalHabitacion ?? 0) || 0,
    };
  }

  private unwrapItem(response: unknown) {
    if (!response || typeof response !== 'object') {
      return null;
    }

    const record = response as Record<string, unknown>;
    if ('reservaId' in record || 'ReservaId' in record || 'codigoReserva' in record || 'CodigoReserva' in record) {
      return response as any;
    }

    const collection = response as {
      data?: unknown;
      Data?: unknown;
      value?: unknown;
      Value?: unknown;
      items?: unknown;
      Items?: unknown;
    };
    if (Array.isArray(collection.data)) {
      return collection.data[0] ?? null;
    }

    if (Array.isArray(collection.Data)) {
      return collection.Data[0] ?? null;
    }

    if (Array.isArray(collection.value)) {
      return collection.value[0] ?? null;
    }

    if (Array.isArray(collection.Value)) {
      return collection.Value[0] ?? null;
    }

    if (Array.isArray(collection.items)) {
      return collection.items[0] ?? null;
    }

    if (Array.isArray(collection.Items)) {
      return collection.Items[0] ?? null;
    }

    return response as any;
  }
}
