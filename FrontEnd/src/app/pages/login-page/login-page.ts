import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { UserRole } from '../../models/auth.model';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
})
export class LoginPageComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  email = 'demo@locushome.com';
  password = 'Demo1234';
  role: UserRole = 'cliente';
  info = 'Puedes entrar en modo demo para cliente o socio aunque el backend de auth siga en stub.';

  submit() {
    this.authService.login({ email: this.email, password: this.password }, this.role).subscribe(() => {
      void this.router.navigateByUrl(this.role === 'socio' ? '/socio' : '/explorar');
    });
  }
}
