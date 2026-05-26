import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

import { FacturaResumen, MetodoPago } from '../../models/factura.model';
import { ReservaResumen } from '../../models/reserva.model';
import { AuthService } from '../../services/auth.service';
import { FacturacionService } from '../../services/facturacion.service';
import { ReservasService } from '../../services/reservas.service';

@Component({
  selector: 'app-reservations-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './reservations-page.html',
  styleUrl: './reservations-page.css',
})
export class ReservationsPageComponent {
  private readonly authService = inject(AuthService);
  private readonly reservasService = inject(ReservasService);
  private readonly facturacionService = inject(FacturacionService);
  private readonly cdr = inject(ChangeDetectorRef);

  reservations: ReservaResumen[] = [];
  paymentMethods: MetodoPago[] = [];
  invoiceByReservationId: Record<number, FacturaResumen> = {};
  loading = true;
  loadingPaymentMethods = false;
  activePaymentReservationId: number | null = null;
  paymentLoadingReservationId: number | null = null;
  paymentMessage = '';
  paymentError = '';
  paymentForm = {
    metodoPagoId: null as number | null,
    fechaPago: this.getToday(),
  };

  constructor() {
    this.loadReservations();
    this.loadPaymentMethods();
  }

  openPayment(reservation: ReservaResumen) {
    this.activePaymentReservationId = reservation.reservaId;
    this.paymentMessage = '';
    this.paymentError = '';
    this.paymentForm = {
      metodoPagoId: this.paymentMethods[0]?.metodoPagoId ?? null,
      fechaPago: this.getToday(),
    };
  }

  closePayment() {
    this.activePaymentReservationId = null;
    this.paymentLoadingReservationId = null;
    this.paymentMessage = '';
    this.paymentError = '';
  }

  canPay(reservation: ReservaResumen) {
    const status = reservation.estado.trim().toLowerCase();
    return (
      status !== 'pagado' &&
      status !== 'confirmada' &&
      status !== 'confirmado' &&
      status !== 'pendiente de confirmacion'
    );
  }

  submitPayment(reservation: ReservaResumen) {
    this.paymentMessage = '';
    this.paymentError = '';
    this.paymentLoadingReservationId = reservation.reservaId;
    const selectedMethod =
      this.paymentMethods.find((method) => method.metodoPagoId === this.paymentForm.metodoPagoId) ?? null;
    const isCashPayment = selectedMethod?.tipo?.trim().toLowerCase() === 'efectivo';

    if (isCashPayment) {
      this.reservasService
        .updateReservationStatus(reservation.reservaId, 'Pendiente de confirmacion')
        .pipe(
          switchMap(() =>
            forkJoin({
              reservations: this.getReservationsRequest(),
            }),
          ),
        )
        .subscribe({
          next: ({ reservations }) => {
            this.invoiceByReservationId[reservation.reservaId] = {
              facturaId: 0,
              reservaId: reservation.reservaId,
              estado: 'Pendiente',
              montoTotal: reservation.total,
              moneda: reservation.moneda,
              existe: false,
              metodoPagoId: selectedMethod?.metodoPagoId ?? null,
              metodoPagoTipo: selectedMethod?.tipo ?? selectedMethod?.nombre ?? 'Efectivo',
              fechaPago: this.paymentForm.fechaPago ? `${this.paymentForm.fechaPago}T00:00:00` : null,
            };
            this.reservations = reservations.map((item) =>
              item.reservaId === reservation.reservaId
                ? { ...item, estado: 'Pendiente de confirmacion' }
                : item,
            );
            this.paymentLoadingReservationId = null;
            this.paymentMessage = 'Pago en efectivo registrado. Queda pendiente la confirmacion del socio.';
            this.cdr.detectChanges();
          },
          error: () => {
            this.paymentLoadingReservationId = null;
            this.paymentError = 'No fue posible registrar el pago en efectivo.';
            this.cdr.detectChanges();
          },
        });

      return;
    }

    this.facturacionService
      .getSummaryByReserva(reservation.reservaId)
      .pipe(
        switchMap((existingInvoice) => {
          if (existingInvoice.existe && existingInvoice.facturaId > 0) {
            return of(existingInvoice);
          }

          return this.facturacionService.createInvoice({
            reservaId: reservation.reservaId,
            metodoPagoId: this.paymentForm.metodoPagoId,
            monto: reservation.total,
            fechaPago: this.paymentForm.fechaPago ? `${this.paymentForm.fechaPago}T00:00:00` : null,
            detalles: [
              {
                descripcion: `Reserva ${reservation.codigoReserva ?? `#${reservation.reservaId}`}`,
                cantidad: 1,
                precioUnitario: reservation.total,
              },
            ],
          });
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
                reservations: this.getReservationsRequest(),
              }),
            ),
          ),
        ),
      )
      .subscribe({
        next: ({ invoice, reservations }) => {
          this.invoiceByReservationId[reservation.reservaId] = {
            ...invoice,
            estado: 'Pagada',
            existe: true,
          };
          this.reservations = reservations.map((item) =>
            item.reservaId === reservation.reservaId ? { ...item, estado: 'Pagado' } : item,
          );
          this.paymentLoadingReservationId = null;
          this.paymentMessage = 'Pago registrado y factura generada correctamente.';
          this.cdr.detectChanges();
        },
        error: () => {
          this.paymentLoadingReservationId = null;
          this.paymentError = 'No fue posible completar el pago de esta reserva.';
          this.cdr.detectChanges();
        },
      });
  }

  private loadReservations() {
    this.getReservationsRequest().subscribe((items) => {
      this.reservations = items;
      this.loading = false;
      this.loadInvoiceSummaries(items);
      this.cdr.detectChanges();
    });
  }

  private getReservationsRequest() {
    const session = this.authService.session();
    return this.reservasService.getByCliente(session?.clienteId, { demoMode: session?.demoMode });
  }

  private loadPaymentMethods() {
    this.loadingPaymentMethods = true;
    this.facturacionService.getPaymentMethods().subscribe({
      next: (methods) => {
        this.paymentMethods = methods;
        this.loadingPaymentMethods = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.paymentMethods = [];
        this.loadingPaymentMethods = false;
        this.cdr.detectChanges();
      },
    });
  }

  private loadInvoiceSummaries(reservations: ReservaResumen[]) {
    if (!reservations.length) {
      this.invoiceByReservationId = {};
      return;
    }

    forkJoin(
      reservations.map((reservation) =>
        this.facturacionService.getSummaryByReserva(reservation.reservaId).pipe(
          catchError(() =>
            of({
              facturaId: 0,
              reservaId: reservation.reservaId,
              estado: 'Pendiente',
              montoTotal: reservation.total,
              moneda: reservation.moneda,
              existe: false,
            }),
          ),
        ),
      ),
    ).subscribe((summaries) => {
      this.invoiceByReservationId = summaries.reduce<Record<number, FacturaResumen>>((acc, item) => {
        acc[item.reservaId] = item;
        return acc;
      }, {});
      this.cdr.detectChanges();
    });
  }

  private getToday() {
    return new Date().toISOString().slice(0, 10);
  }
}
