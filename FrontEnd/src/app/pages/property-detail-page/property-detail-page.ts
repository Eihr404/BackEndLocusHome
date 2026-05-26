import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AlojamientoCard, Habitacion } from '../../models/alojamiento.model';
import { ReservaAlojamientoDetalle } from '../../models/reserva.model';
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
  reservationsByProperty: ReservaAlojamientoDetalle[] = [];
  loadingProperty = true;
  loadingRooms = true;
  availabilityLoading = false;
  bookingMessage = '';
  bookingError = '';
  bookingLoading = false;
  selectedRoomId: number | null = null;
  showClientProfileFields = false;
  unavailableRoomIds = new Set<number>();

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
        this.reservationsByProperty = [];
        this.loadingProperty = false;
        this.loadingRooms = false;
        this.availabilityLoading = false;
        this.unavailableRoomIds = new Set<number>();
        this.cdr.detectChanges();
        return;
      }

      this.loadingProperty = true;
      this.loadingRooms = true;
      this.availabilityLoading = false;
      this.bookingMessage = '';
      this.bookingError = '';
      this.selectedRoomId = null;
      this.showClientProfileFields = false;
      this.unavailableRoomIds = new Set<number>();
      this.reservationsByProperty = [];

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

  get precioMinimo(): number | null {
    if (!this.rooms.length) return null;
    const precios = this.rooms.map((room) => room.precioNoche).filter((price) => price > 0);
    return precios.length ? Math.min(...precios) : null;
  }

  selectRoom(room: Habitacion) {
    if (this.isRoomUnavailable(room)) {
      return;
    }

    this.selectedRoomId = room.habitacionId;
  }

  onDateRangeChange() {
    const property = this.property;
    if (!property) {
      return;
    }

    if (!this.bookingForm.fechaCheckIn || !this.bookingForm.fechaCheckOut) {
      this.unavailableRoomIds = new Set<number>();
      this.cdr.detectChanges();
      return;
    }

    const nights = this.calculateNights(this.bookingForm.fechaCheckIn, this.bookingForm.fechaCheckOut);
    if (nights <= 0) {
      this.unavailableRoomIds = new Set<number>();
      this.cdr.detectChanges();
      return;
    }

    this.availabilityLoading = true;

    this.reservasService.getDetailedByAlojamiento(property.alojamientoId).subscribe({
      next: (reservations) => {
        this.reservationsByProperty = reservations;
        this.unavailableRoomIds = this.getUnavailableRoomsForRange(
          reservations,
          this.bookingForm.fechaCheckIn,
          this.bookingForm.fechaCheckOut,
        );

        if (this.selectedRoomId && this.unavailableRoomIds.has(this.selectedRoomId)) {
          this.selectedRoomId = null;
        }

        this.availabilityLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.availabilityLoading = false;
        this.cdr.detectChanges();
      },
    });
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

    if (this.isRoomUnavailable(room)) {
      this.bookingError = 'La habitacion seleccionada ya no esta disponible para esas fechas.';
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

  isRoomUnavailable(room: Habitacion) {
    return this.unavailableRoomIds.has(room.habitacionId);
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

    this.reservasService.getDetailedByAlojamiento(alojamientoId).subscribe({
      next: (reservations) => {
        this.reservationsByProperty = reservations;
        this.unavailableRoomIds = this.getUnavailableRoomsForRange(
          reservations,
          this.bookingForm.fechaCheckIn,
          this.bookingForm.fechaCheckOut,
        );

        if (this.unavailableRoomIds.has(room.habitacionId)) {
          this.bookingLoading = false;
          this.selectedRoomId = null;
          this.bookingError = 'La habitacion ya fue reservada para esas fechas. Elige otra disponible.';
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
              this.unavailableRoomIds = new Set([...this.unavailableRoomIds, room.habitacionId]);
              this.selectedRoomId = null;
              this.cdr.detectChanges();
              void this.router.navigateByUrl('/mis-reservas');
            },
            error: () => {
              this.bookingLoading = false;
              this.bookingError = 'No fue posible completar la reserva con el gateway.';
              this.cdr.detectChanges();
            },
          });
      },
      error: () => {
        this.bookingLoading = false;
        this.bookingError = 'No fue posible verificar la disponibilidad actual de la habitacion.';
        this.cdr.detectChanges();
      },
    });
  }

  private getUnavailableRoomsForRange(
    reservations: ReservaAlojamientoDetalle[],
    checkIn: string,
    checkOut: string,
  ) {
    const selectedStart = new Date(checkIn);
    const selectedEnd = new Date(checkOut);
    const blockedStates = new Set(['pendiente', 'pendiente de confirmacion', 'confirmada', 'confirmado', 'pagado']);
    const roomIds = new Set<number>();

    reservations.forEach((reservation) => {
      const status = reservation.estado.trim().toLowerCase();
      if (!blockedStates.has(status)) {
        return;
      }

      const reservationStart = new Date(reservation.fechaCheckIn);
      const reservationEnd = new Date(reservation.fechaCheckOut);
      const overlaps = selectedStart < reservationEnd && selectedEnd > reservationStart;

      if (!overlaps) {
        return;
      }

      reservation.detallesHabitacion.forEach((detail: { habitacionId: number }) => roomIds.add(detail.habitacionId));
    });

    return roomIds;
  }
}
