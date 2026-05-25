import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register-page',
  imports: [CommonModule, FormsModule],
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

  submit() {
    this.authService.register(this.form).subscribe(() => {
      this.feedback =
        this.form.rol === 'socio'
          ? 'Cuenta de socio preparada en Usuarios. Si el backend no responde, se conserva como simulacion demo.'
          : 'Cuenta de cliente preparada en Clientes y Usuarios. Si el backend no responde, se conserva como simulacion demo.';
    });
  }
}
