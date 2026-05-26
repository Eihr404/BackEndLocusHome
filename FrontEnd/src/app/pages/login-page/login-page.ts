import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

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

  email = '';
  password = ''
  info = 'Accede con un usuario real registrado en la API.';
  error = '';
  loading = false;

  submit() {
    this.loading = true;
    this.error = '';

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (session) => {
        // Si es cliente y no tiene clienteId, buscarlo en el microservicio de clientes
        if (session.role === 'cliente' && !session.clienteId) {
          this.authService
            .ensureClientProfile({
              usuarioId: session.usuarioId ?? 0,
              email: session.email,
              nombreCompleto: session.nombreCompleto ?? session.email,
            })
            .subscribe({
              next: () => {
                this.loading = false;
                void this.router.navigateByUrl('/explorar');
              },
              error: () => {
                // Aunque falle, dejamos pasar — tendrá reservas vacías pero puede navegar
                console.warn('[Login] No se pudo obtener clienteId, reservas no disponibles');
                this.loading = false;
                void this.router.navigateByUrl('/explorar');
              },
            });
        } else {
          this.loading = false;
          void this.router.navigateByUrl(session.role === 'socio' ? '/socio' : '/explorar');
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.error =
          error.status === 401
            ? 'Correo o contrasena incorrectos.'
            : 'No fue posible iniciar sesion con la API de usuarios.';
      },
    });
  }
}