import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-public-layout',
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.css',
})
export class PublicLayoutComponent {
  private readonly authService = inject(AuthService);

  readonly session = this.authService.session;
  readonly ctaLink = computed(() => (this.session()?.role === 'socio' ? '/socio' : '/explorar'));

  logout() {
    this.authService.logout();
  }
}
