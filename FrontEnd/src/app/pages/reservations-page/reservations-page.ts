import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';

import { ReservaResumen } from '../../models/reserva.model';
import { ReservasService } from '../../services/reservas.service';

@Component({
  selector: 'app-reservations-page',
  imports: [CommonModule],
  templateUrl: './reservations-page.html',
  styleUrl: './reservations-page.css',
})
export class ReservationsPageComponent {
  private readonly reservasService = inject(ReservasService);

  reservations: ReservaResumen[] = [];

  constructor() {
    this.reservasService.getByCliente(1).subscribe((items) => {
      this.reservations = items;
    });
  }
}
