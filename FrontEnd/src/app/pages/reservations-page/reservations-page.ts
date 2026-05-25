import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';

import { ReservaResumen } from '../../models/reserva.model';
import { AuthService } from '../../services/auth.service';
import { ReservasService } from '../../services/reservas.service';

@Component({
  selector: 'app-reservations-page',
  imports: [CommonModule],
  templateUrl: './reservations-page.html',
  styleUrl: './reservations-page.css',
})
export class ReservationsPageComponent {
  private readonly authService = inject(AuthService);
  private readonly reservasService = inject(ReservasService);

  reservations: ReservaResumen[] = [];
  loading = true;

  constructor() {
    const session = this.authService.session();

    this.reservasService.getByCliente(session?.clienteId, { demoMode: session?.demoMode }).subscribe((items) => {
      this.reservations = items;
      this.loading = false;
    });
  }
}
