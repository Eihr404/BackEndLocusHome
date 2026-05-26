import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, of } from 'rxjs';

import { CrearFacturaRequest, FacturaResumen, MetodoPago } from '../models/factura.model';
import { FACTURACION_API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class FacturacionService {
  private readonly http = inject(HttpClient);

  getSummaryByReserva(reservaId: number) {
    return this.http
      .get<unknown>(`${FACTURACION_API_BASE_URL}/Facturas/resumen/reserva/${reservaId}`)
      .pipe(map((response) => this.normalizeSummary(this.unwrapItem(response), reservaId)));
  }

  getPaymentMethods() {
    return this.http
      .get<unknown>(`${FACTURACION_API_BASE_URL}/MetodosPago`)
      .pipe(map((response) => this.unwrapCollection(response).map((item) => this.normalizePaymentMethod(item))));
  }

  createInvoice(payload: CrearFacturaRequest) {
    return this.http
      .post<unknown>(`${FACTURACION_API_BASE_URL}/Facturas`, payload)
      .pipe(map((response) => this.normalizeSummary(this.unwrapItem(response), payload.reservaId)));
  }

  approveInvoice(facturaId: number) {
    return this.http.patch(`${FACTURACION_API_BASE_URL}/Facturas/${facturaId}/aprobar`, {});
  }

  private unwrapItem(response: unknown) {
    if (!response || typeof response !== 'object') {
      return null;
    }

    if ('facturaId' in (response as object) || 'FacturaId' in (response as object)) {
      return response;
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

    return null;
  }

  private normalizeSummary(item: any, reservaId: number): FacturaResumen {
    if (!item) {
      return {
        facturaId: 0,
        reservaId,
        estado: 'Pendiente',
        montoTotal: 0,
        moneda: 'USD',
        existe: false,
      };
    }

    return {
      facturaId: item.facturaId ?? item.FacturaId ?? 0,
      reservaId: item.reservaId ?? item.ReservaId ?? reservaId,
      estado: item.estado ?? item.Estado ?? 'Pendiente',
      montoTotal: Number(item.montoTotal ?? item.MontoTotal ?? item.total ?? item.Total ?? 0) || 0,
      moneda: item.moneda ?? item.Moneda ?? 'USD',
      existe: true,
    };
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

  private normalizePaymentMethod(item: any): MetodoPago {
    const tipo = item.tipo ?? item.Tipo ?? item.nombre ?? item.Nombre ?? item.descripcion ?? item.Descripcion;

    return {
      metodoPagoId: item.metodoPagoId ?? item.MetodoPagoId ?? item.id ?? item.Id ?? 0,
      nombre: tipo ?? 'Metodo de pago',
      tipo: tipo ?? undefined,
      descripcion: item.descripcion ?? item.Descripcion ?? undefined,
    };
  }
}
