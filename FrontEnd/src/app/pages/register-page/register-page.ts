import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register-page.html',
  styleUrl: './register-page.css',
})
export class RegisterPageComponent {
  private readonly authService = inject(AuthService);

  form = {
    email: '',
    password: '',
    nombreCompleto: '',
    cedula: '',
    telefono: '',
    domicilio: '',
    rol: 'cliente' as 'cliente' | 'socio',
  };

  feedback = '';
  error = '';
  loading = false;
  created = false;

  submit() {
    this.feedback = '';
    this.error = '';
    this.loading = true;
    this.created = false;

    this.authService.register(this.form).subscribe({
      next: () => {
        this.loading = false;
        this.created = true;
        this.feedback = 'Usuario creado con exito. Ya puedes iniciar sesion con tu correo y contrasena.';
      },
      error: (error) => {
        this.loading = false;
        this.error =
          error?.error?.message ??
          error?.error?.mensaje ??
          error?.message ??
          'No fue posible crear el usuario. Revisa los datos e intenta nuevamente.';
      },
    });
  }
}
