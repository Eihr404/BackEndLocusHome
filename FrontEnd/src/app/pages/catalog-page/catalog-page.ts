import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AlojamientoCard } from '../../models/alojamiento.model';
import { AlojamientosService } from '../../services/alojamientos.service';

@Component({
  selector: 'app-catalog-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './catalog-page.html',
  styleUrl: './catalog-page.css',
})
export class CatalogPageComponent {
  private readonly alojamientosService = inject(AlojamientosService);

  search = '';
  city = '';
  loading = true;
  properties: AlojamientoCard[] = [];

  constructor() {
    this.loadCatalog();
  }

  loadCatalog() {
    this.loading = true;
    this.alojamientosService.getCatalog({ city: this.city, search: this.search }).subscribe((items) => {
      this.properties = items;
      this.loading = false;
    });
  }
}
