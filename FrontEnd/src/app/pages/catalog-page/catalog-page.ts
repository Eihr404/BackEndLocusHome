import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
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
export class CatalogPageComponent implements OnInit {
  private readonly alojamientosService = inject(AlojamientosService);
  private readonly cdr = inject(ChangeDetectorRef);

  search = '';
  city = '';
  loading = true;

  private allProperties: AlojamientoCard[] = [];

  get properties(): AlojamientoCard[] {
    const searchLower = this.search.trim().toLowerCase();
    const cityLower = this.city.trim().toLowerCase();

    return this.allProperties.filter((item) => {
      const matchSearch = searchLower
        ? `${item.nombre} ${item.descripcion ?? ''}`.toLowerCase().includes(searchLower)
        : true;
      const matchCity = cityLower
        ? item.ciudad.toLowerCase().includes(cityLower)
        : true;
      return matchSearch && matchCity;
    });
  }

  ngOnInit() {
    this.loadCatalog();
  }

  loadCatalog() {
    this.loading = true;
    this.alojamientosService.getCatalog().subscribe((items) => {
      this.allProperties = items;
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  onFilterChange() {
    this.cdr.detectChanges();
  }
}