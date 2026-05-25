import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-partner-layout',
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './partner-layout.html',
  styleUrl: './partner-layout.css',
})
export class PartnerLayoutComponent {
  private readonly authService = inject(AuthService);
  readonly session = this.authService.session;

  logout() {
    this.authService.logout();
  }
}
