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
  password = '';
  info = 'Accede con un usuario real registrado en la API.';
  error = '';
  loading = false;

  submit() {
    this.loading = true;
    this.error = '';

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (session) => {
        console.log('[Login] sesion obtenida:', session);

        if (session.role === 'cliente' && !session.clienteId) {
          // El backend de login no devolvió clienteId — lo buscamos en el microservicio de clientes
          this.authService
            .ensureClientProfile({
              usuarioId: session.usuarioId ?? 0,
              email: session.email,
              nombreCompleto: session.nombreCompleto ?? session.email,
            })
            .subscribe({
              next: (profile) => {
                console.log('[Login] clienteId obtenido tras ensureClientProfile:', profile);
                this.loading = false;
                void this.router.navigateByUrl('/explorar');
              },
              error: (err) => {
                console.warn('[Login] ensureClientProfile falló:', err);
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