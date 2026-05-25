import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, map, of } from 'rxjs';

import { FacturaResumen } from '../models/factura.model';
import { FACTURACION_API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class FacturacionService {
  private readonly http = inject(HttpClient);

  getSummaryByReserva(reservaId: number) {
    return this.http
      .get<unknown>(`${FACTURACION_API_BASE_URL}/Facturas/resumen/reserva/${reservaId}`)
      .pipe(
        map((response) => this.normalizeSummary(this.unwrapItem(response), reservaId)),
        catchError(() =>
          of({
            facturaId: 9000 + reservaId,
            reservaId,
            estado: 'Pendiente',
            montoTotal: 0,
            moneda: 'USD',
          }),
        ),
      );
  }

  private unwrapItem(response: unknown) {
    if (!response || typeof response !== 'object') {
      return null;
    }

    if ('facturaId' in (response as object) || 'FacturaId' in (response as object)) {
      return response;
    }

    const collection = response as { data?: unknown; value?: unknown; items?: unknown };

    if (Array.isArray(collection.data)) {
      return collection.data[0] ?? null;
    }

    if (Array.isArray(collection.value)) {
      return collection.value[0] ?? null;
    }

    if (Array.isArray(collection.items)) {
      return collection.items[0] ?? null;
    }

    return null;
  }

  private normalizeSummary(item: any, reservaId: number): FacturaResumen {
    if (!item) {
      return {
        facturaId: 9000 + reservaId,
        reservaId,
        estado: 'Pendiente',
        montoTotal: 0,
        moneda: 'USD',
      };
    }

    return {
      facturaId: item.facturaId ?? item.FacturaId ?? 9000 + reservaId,
      reservaId: item.reservaId ?? item.ReservaId ?? reservaId,
      estado: item.estado ?? item.Estado ?? 'Pendiente',
      montoTotal: Number(item.montoTotal ?? item.MontoTotal ?? item.total ?? item.Total ?? 0) || 0,
      moneda: item.moneda ?? item.Moneda ?? 'USD',
    };
  }
}
