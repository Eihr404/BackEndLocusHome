import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './home-page.html',
  styleUrl: './home-page.css',
})
export class HomePageComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  readonly session = this.authService.session;

  search = '';
  city = '';

  readonly featuredCities = [
    { name: 'Quito', detail: 'Escapadas urbanas y hostales centricos' },
    { name: 'Cuenca', detail: 'Casas historicas y suites tranquilas' },
    { name: 'Manta', detail: 'Hoteles cerca del mar y descanso familiar' },
  ];

  readonly stayTypes = [
    'Hoteles',
    'Departamentos',
    'Hostales',
    'Casas completas',
  ];

  searchStays() {
    const queryParams: Record<string, string> = {};

    if (this.search.trim()) {
      queryParams['search'] = this.search.trim();
    }

    if (this.city.trim()) {
      queryParams['city'] = this.city.trim();
    }

    void this.router.navigate(['/explorar'], { queryParams });
  }
}
