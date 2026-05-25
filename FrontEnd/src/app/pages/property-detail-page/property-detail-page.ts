import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { AlojamientoCard, Habitacion } from '../../models/alojamiento.model';
import { AlojamientosService } from '../../services/alojamientos.service';

@Component({
  selector: 'app-property-detail-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './property-detail-page.html',
  styleUrl: './property-detail-page.css',
})
export class PropertyDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly alojamientosService = inject(AlojamientosService);

  property: AlojamientoCard | null = null;
  rooms: Habitacion[] = [];

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.alojamientosService.getById(id).subscribe((property) => {
      this.property = property;
    });

    this.alojamientosService.getRoomsByProperty(id).subscribe((rooms) => {
      this.rooms = rooms;
    });
  }
}
