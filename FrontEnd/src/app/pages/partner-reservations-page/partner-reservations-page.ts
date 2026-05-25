import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';

import { ReservasService } from '../../services/reservas.service';

@Component({
  selector: 'app-partner-reservations-page',
  imports: [CommonModule],
  templateUrl: './partner-reservations-page.html',
  styleUrl: './partner-reservations-page.css',
})
export class PartnerReservationsPageComponent {
  private readonly reservasService = inject(ReservasService);

  reservations$ = this.reservasService.getPartnerReservations();
}
