import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';

import { AlojamientosService } from '../../services/alojamientos.service';
import { AuthService } from '../../services/auth.service';
import { ReservasService } from '../../services/reservas.service';
import { ReservaResumen } from '../../models/reserva.model';

@Component({
  selector: 'app-partner-reservations-page',
  imports: [CommonModule],
  templateUrl: './partner-reservations-page.html',
  styleUrl: './partner-reservations-page.css',
})
export class PartnerReservationsPageComponent {
  private readonly authService = inject(AuthService);
  private readonly alojamientosService = inject(AlojamientosService);
  private readonly reservasService = inject(ReservasService);
  private readonly cdr = inject(ChangeDetectorRef);

  reservations: ReservaResumen[] = [];
  loading = true;
  message = '';

  constructor() {
    const session = this.authService.session();
    const socioId = session?.role === 'socio' ? session.usuarioId : null;

    if (!socioId) {
      this.loading = false;
      this.message = 'No se pudo identificar el socio autenticado.';
      return;
    }

    this.alojamientosService.getPartnerProperties(socioId).subscribe({
      next: (properties) => {
        if (!properties.length) {
          this.reservations = [];
          this.loading = false;
          this.message = '';
          this.cdr.detectChanges();
          return;
        }

        this.reservasService
          .getByAlojamientos(
            properties.map((property) => ({
              alojamientoId: property.alojamientoId,
              nombre: property.nombre,
            })),
          )
          .subscribe({
            next: (reservations) => {
              this.reservations = reservations;
              this.loading = false;
              this.message = '';
              this.cdr.detectChanges();
            },
            error: () => {
              this.loading = false;
              this.message = 'No fue posible cargar las reservas de tus alojamientos.';
              this.cdr.detectChanges();
            },
          });
      },
      error: () => {
        this.loading = false;
        this.message = 'No fue posible cargar los alojamientos del socio.';
        this.cdr.detectChanges();
      },
    });
  }
}
