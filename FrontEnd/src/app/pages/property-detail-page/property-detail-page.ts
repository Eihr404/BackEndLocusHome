import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AlojamientoCard, Habitacion } from '../../models/alojamiento.model';
import { AuthService } from '../../services/auth.service';
import { AlojamientosService } from '../../services/alojamientos.service';
import { ReservasService } from '../../services/reservas.service';

@Component({
  selector: 'app-property-detail-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './property-detail-page.html',
  styleUrl: './property-detail-page.css',
})
export class PropertyDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly alojamientosService = inject(AlojamientosService);
  private readonly reservasService = inject(ReservasService);
  private readonly cdr = inject(ChangeDetectorRef);

  property: AlojamientoCard | null = null;
  rooms: Habitacion[] = [];
  loadingProperty = true;
  loadingRooms = true;
  bookingMessage = '';
  bookingError = '';
  bookingLoading = false;
  selectedRoomId: number | null = null;
  showClientProfileFields = false;

  bookingForm = {
    fechaCheckIn: '',
    fechaCheckOut: '',
    numAdultos: 2,
    numNinos: 0,
    llevaMascotas: false,
    codigoDescuento: '',
  };
  clientProfileForm = {
    cedula: '',
    telefono: '',
    domicilio: '',
  };

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!id) {
        this.property = null;
        this.rooms = [];
        this.loadingProperty = false;
        this.loadingRooms = false;
        this.cdr.detectChanges();
        return;
      }

      this.loadingProperty = true;
      this.loadingRooms = true;
      this.bookingMessage = '';
      this.bookingError = '';
      this.selectedRoomId = null;
      this.showClientProfileFields = false;

      this.alojamientosService.getById(id).subscribe((property) => {
        this.property = property;
        this.loadingProperty = false;
        this.cdr.detectChanges();
      });

      this.alojamientosService.getRoomsByProperty(id).subscribe({
        next: (rooms) => {
          this.rooms = rooms;
          this.selectedRoomId = rooms[0]?.habitacionId ?? null;
          this.loadingRooms = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingRooms = false;
          this.cdr.detectChanges();
        },
      });
    });
  }

  selectRoom(room: Habitacion) {
    this.selectedRoomId = room.habitacionId;
  }

  reserveSelectedRoom() {
    const session = this.authService.session();
    const property = this.property;
    const room = this.rooms.find((item) => item.habitacionId === this.selectedRoomId);

    this.bookingMessage = '';
    this.bookingError = '';

    if (!session) {
      this.bookingError = 'Necesitas iniciar sesion para reservar.';
      return;
    }

    if (!session.usuarioId) {
      this.bookingError = 'No se pudo identificar tu cuenta actual para reservar.';
      return;
    }

    if (!property || !room) {
      this.bookingError = 'Selecciona una habitacion disponible antes de reservar.';
      return;
    }

    if (!this.bookingForm.fechaCheckIn || !this.bookingForm.fechaCheckOut) {
      this.bookingError = 'Debes seleccionar fecha de ingreso y salida.';
      return;
    }

    const nights = this.calculateNights(this.bookingForm.fechaCheckIn, this.bookingForm.fechaCheckOut);
    if (nights <= 0) {
      this.bookingError = 'La fecha de salida debe ser posterior a la fecha de ingreso.';
      return;
    }

    this.bookingLoading = true;

    if (!session.clienteId) {
      this.authService
        .ensureClientProfile({
          usuarioId: session.usuarioId,
          email: session.email,
          nombreCompleto: session.nombreCompleto,
          cedula: this.clientProfileForm.cedula || undefined,
          telefono: this.clientProfileForm.telefono || undefined,
          domicilio: this.clientProfileForm.domicilio || undefined,
        })
        .subscribe({
          next: (profile) => {
            this.showClientProfileFields = false;
            this.submitReservation(profile.clienteId ?? session.clienteId ?? null, property.alojamientoId, room, nights);
          },
          error: (error) => {
            this.bookingLoading = false;
            this.showClientProfileFields = true;
            this.bookingError =
              error?.error?.message ??
              error?.error?.Message ??
              'Completa tus datos de cliente para poder reservar.';
            this.cdr.detectChanges();
          },
        });
      return;
    }

    this.submitReservation(session.clienteId, property.alojamientoId, room, nights);
  }

  private calculateNights(checkIn: string, checkOut: string) {
    const start = new Date(checkIn);
    const end = new Date(checkOut);
    const diffMs = end.getTime() - start.getTime();
    return Math.ceil(diffMs / (1000 * 60 * 60 * 24));
  }

  private submitReservation(
    clienteId: number | null | undefined,
    alojamientoId: number,
    room: Habitacion,
    nights: number,
  ) {
    if (!clienteId) {
      this.bookingLoading = false;
      this.showClientProfileFields = true;
      this.bookingError = 'Completa tus datos de cliente para poder reservar.';
      this.cdr.detectChanges();
      return;
    }

    this.reservasService
      .createReservation({
        clienteId,
        alojamientoId,
        fechaCheckIn: this.bookingForm.fechaCheckIn,
        fechaCheckOut: this.bookingForm.fechaCheckOut,
        numAdultos: this.bookingForm.numAdultos,
        numNinos: this.bookingForm.numNinos,
        llevaMascotas: this.bookingForm.llevaMascotas,
        codigoDescuento: this.bookingForm.codigoDescuento || '',
        habitaciones: [
          {
            habitacionId: room.habitacionId,
            precioPorNoche: room.precioNoche,
            numNoches: nights,
          },
        ],
      })
      .subscribe({
        next: (reservation) => {
          this.bookingLoading = false;
          this.bookingMessage = `Reserva creada con codigo ${reservation.codigoReserva ?? `#${reservation.reservaId}`}.`;
          this.cdr.detectChanges();
          void this.router.navigateByUrl('/mis-reservas');
        },
        error: () => {
          this.bookingLoading = false;
          this.bookingError = 'No fue posible completar la reserva con el gateway.';
          this.cdr.detectChanges();
        },
      });
  }
}
