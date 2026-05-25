import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, of } from 'rxjs';

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

  getByCliente(clienteId: number) {
    return this.http
      .get<ReservaResumen[]>(`${RESERVAS_API_BASE_URL}/Reservas/resumen/cliente/${clienteId}`)
      .pipe(catchError(() => of(MOCK_RESERVAS)));
  }

  getPartnerReservations() {
    return of(MOCK_RESERVAS);
  }
}
