import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './home-page.html',
  styleUrl: './home-page.css',
})
export class HomePageComponent {
  private readonly authService = inject(AuthService);
  readonly session = this.authService.session;

  readonly customerSteps = [
    'Explorar alojamientos filtrando por ciudad, tipo y rango esperado.',
    'Revisar detalle, habitaciones y precio por noche antes de reservar.',
    'Consultar tus reservas y su estado desde una vista unificada.',
  ];

  readonly partnerSteps = [
    'Publicar alojamientos y mantener el inventario desde un panel propio.',
    'Revisar flujo de reservas y datos clave sin salir del mismo frontend.',
    'Escalar luego a reportes y facturacion cuando el backend madure.',
  ];
}
