import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

import { FacturaResumen } from '../../models/factura.model';
import { AlojamientoCard } from '../../models/alojamiento.model';
import { AlojamientosService } from '../../services/alojamientos.service';
import { AuthService } from '../../services/auth.service';
import { FacturacionService } from '../../services/facturacion.service';
import { ReservasService } from '../../services/reservas.service';
import { ReservaResumen } from '../../models/reserva.model';

@Component({
  selector: 'app-partner-reservations-page',
  imports: [CommonModule],
  templateUrl: './partner-reservations-page.html',
  styleUrl: './partner-reservations-page.css',
})
export class PartnerReservationsPageComponent {
  private readonly authService = inject(AuthService);
  private readonly alojamientosService = inject(AlojamientosService);
  private readonly reservasService = inject(ReservasService);
  private readonly facturacionService = inject(FacturacionService);
  private readonly cdr = inject(ChangeDetectorRef);

  reservations: ReservaResumen[] = [];
  propertiesById: Record<number, AlojamientoCard> = {};
  invoiceByReservationId: Record<number, FacturaResumen> = {};
  loading = true;
  confirmingReservationId: number | null = null;
  message = '';

  constructor() {
    const session = this.authService.session();
    const socioId = session?.role === 'socio' ? session.usuarioId : null;

    if (!socioId) {
      this.loading = false;
      this.message = 'No se pudo identificar el socio autenticado.';
      return;
    }

    this.alojamientosService.getPartnerProperties(socioId).subscribe({
      next: (properties) => {
        this.propertiesById = properties.reduce<Record<number, AlojamientoCard>>((acc, property) => {
          acc[property.alojamientoId] = property;
          return acc;
        }, {});

        if (!properties.length) {
          this.reservations = [];
          this.loading = false;
          this.message = '';
          this.cdr.detectChanges();
          return;
        }

        this.reservasService
          .getByAlojamientos(
            properties.map((property) => ({
              alojamientoId: property.alojamientoId,
              nombre: property.nombre,
            })),
          )
          .subscribe({
            next: (reservations) => {
              this.reservations = reservations;
              this.loadInvoiceSummaries(reservations);
            },
            error: () => {
              this.loading = false;
              this.message = 'No fue posible cargar las reservas de tus alojamientos.';
              this.cdr.detectChanges();
            },
          });
      },
      error: () => {
        this.loading = false;
        this.message = 'No fue posible cargar los alojamientos del socio.';
        this.cdr.detectChanges();
      },
    });
  }

  canConfirmCashReservation(reservation: ReservaResumen) {
    const reservationStatus = reservation.estado.trim().toLowerCase();
    return reservationStatus === 'pendiente de confirmacion';
  }

  confirmCashReservation(reservation: ReservaResumen) {
    this.confirmingReservationId = reservation.reservaId;
    this.message = '';

    this.facturacionService
      .getInvoiceByReserva(reservation.reservaId)
      .pipe(
        switchMap((invoice) => {
          if (invoice.existe && invoice.facturaId > 0) {
            return of(invoice);
          }

          return this.facturacionService.getPaymentMethods().pipe(
            switchMap((methods) => {
              const cashMethod =
                methods.find((method) => (method.tipo ?? method.nombre).trim().toLowerCase() === 'efectivo') ?? null;

              return this.facturacionService.createInvoice({
                reservaId: reservation.reservaId,
                metodoPagoId: cashMethod?.metodoPagoId ?? null,
                monto: reservation.total,
                fechaPago: new Date().toISOString(),
                detalles: [
                  {
                    descripcion: `Reserva ${reservation.codigoReserva ?? `#${reservation.reservaId}`}`,
                    cantidad: 1,
                    precioUnitario: reservation.total,
                  },
                ],
              });
            }),
          );
        }),
        switchMap((invoice) =>
          this.facturacionService.approveInvoice(invoice.facturaId).pipe(
            switchMap(() =>
              this.reservasService.updateReservationStatus(reservation.reservaId, 'Pagado').pipe(
                catchError(() => this.reservasService.updateReservationStatus(reservation.reservaId, 'Confirmada')),
              ),
            ),
            switchMap(() =>
              forkJoin({
                invoice: this.facturacionService.getInvoiceByReserva(reservation.reservaId),
                reservations: this.reservasService.getByAlojamientos(
                  Object.values(this.propertiesById).map((property) => ({
                    alojamientoId: property.alojamientoId,
                    nombre: property.nombre,
                  })),
                ),
              }),
            ),
          ),
        ),
      )
      .subscribe({
        next: ({ invoice: refreshedInvoice, reservations }) => {
          this.invoiceByReservationId[reservation.reservaId] = refreshedInvoice;
          this.reservations = reservations;
          this.confirmingReservationId = null;
          this.message = 'Reserva confirmada y factura aprobada correctamente.';
          this.cdr.detectChanges();
        },
        error: () => {
          this.confirmingReservationId = null;
          this.message = 'No fue posible confirmar la reserva en efectivo.';
          this.cdr.detectChanges();
        },
      });
  }

  private loadInvoiceSummaries(reservations: ReservaResumen[]) {
    if (!reservations.length) {
      this.invoiceByReservationId = {};
      this.loading = false;
      this.message = '';
      this.cdr.detectChanges();
      return;
    }

    forkJoin(
      reservations.map((reservation) =>
        this.facturacionService.getInvoiceByReserva(reservation.reservaId).pipe(
          catchError(() =>
            of({
              facturaId: 0,
              reservaId: reservation.reservaId,
              estado: 'Pendiente',
              montoTotal: reservation.total,
              moneda: reservation.moneda,
              existe: false,
              metodoPagoId: null,
              metodoPagoTipo: null,
              fechaPago: null,
            }),
          ),
        ),
      ),
    ).subscribe({
      next: (summaries) => {
        this.invoiceByReservationId = summaries.reduce<Record<number, FacturaResumen>>((acc, item) => {
          acc[item.reservaId] = item;
          return acc;
        }, {});
        this.loading = false;
        this.message = '';
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.message = 'No fue posible cargar la informacion de facturacion de las reservas.';
        this.cdr.detectChanges();
      },
    });
  }
}
