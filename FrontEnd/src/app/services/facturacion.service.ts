import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, of } from 'rxjs';

import { FacturaResumen } from '../models/factura.model';
import { FACTURACION_API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class FacturacionService {
  private readonly http = inject(HttpClient);

  getSummaryByReserva(reservaId: number) {
    return this.http
      .get<FacturaResumen>(`${FACTURACION_API_BASE_URL}/Facturas/resumen/reserva/${reservaId}`)
      .pipe(
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
}
