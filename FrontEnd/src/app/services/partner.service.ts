import { Injectable, computed, inject, signal } from '@angular/core';

import {
  AlojamientoCard,
  AlojamientoForm,
  FotoAlojamiento,
  FotoAlojamientoForm,
  Habitacion,
  HabitacionForm,
  TipoAlojamientoOption,
} from '../models/alojamiento.model';
import { PartnerMetric } from '../models/dashboard.model';
import { AlojamientosService } from './alojamientos.service';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class PartnerService {
  private readonly alojamientosService = inject(AlojamientosService);
  private readonly authService = inject(AuthService);

  private readonly propertiesSignal = signal<AlojamientoCard[]>([]);
  private readonly selectedPropertySignal = signal<AlojamientoCard | null>(null);
  private readonly roomsSignal = signal<Habitacion[]>([]);
  private readonly photosSignal = signal<FotoAlojamiento[]>([]);
  private readonly tipoOptionsSignal = signal<TipoAlojamientoOption[]>([]);
  private readonly loadingSignal = signal(false);
  private readonly savingSignal = signal(false);
  private readonly messageSignal = signal('');

  readonly properties = computed(() => this.propertiesSignal());
  readonly selectedProperty = computed(() => this.selectedPropertySignal());
  readonly rooms = computed(() => this.roomsSignal());
  readonly photos = computed(() => this.photosSignal());
  readonly loading = computed(() => this.loadingSignal());
  readonly saving = computed(() => this.savingSignal());
  readonly message = computed(() => this.messageSignal());
  readonly tipoOptions = computed(() => this.tipoOptionsSignal());

  readonly metrics = computed<PartnerMetric[]>(() => {
    const properties = this.propertiesSignal();
    const published = properties.length;
    const avgPrice =
      properties.reduce((total, item) => total + item.precioNocheMinimo, 0) / Math.max(published, 1);
    const poolCount = properties.filter((item) => item.tienePiscina).length;

    return [
      { label: 'Propiedades activas', value: `${published}`, detail: 'Inventario del socio conectado a API' },
      { label: 'Tarifa promedio', value: `$${avgPrice.toFixed(0)}`, detail: 'Referencia de catalogo actual' },
      { label: 'Con piscina', value: `${poolCount}`, detail: 'Amenidades registradas en alojamientos' },
    ];
  });

  loadProperties(socioId = this.getCurrentSocioId()) {
    if (!socioId) {
      this.propertiesSignal.set([]);
      this.selectedPropertySignal.set(null);
      this.roomsSignal.set([]);
      this.photosSignal.set([]);
      this.loadingSignal.set(false);
      this.messageSignal.set('');
      return;
    }

    this.loadingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.getPartnerProperties(socioId).subscribe({
      next: (properties) => {
        this.propertiesSignal.set(properties);
        const selected = this.selectedPropertySignal();
        if (selected) {
          const refreshed = properties.find((item) => item.alojamientoId === selected.alojamientoId) ?? null;
          this.selectedPropertySignal.set(refreshed);
          if (refreshed) {
            this.loadRooms(refreshed.alojamientoId);
            this.loadPhotos(refreshed.alojamientoId);
          }
        }
        this.loadingSignal.set(false);
      },
      error: () => {
        this.loadingSignal.set(false);
        this.messageSignal.set('No fue posible cargar las propiedades del socio.');
      },
    });
  }

  loadPropertyTypes() {
    this.alojamientosService.getPropertyTypes().subscribe((types) => {
      this.tipoOptionsSignal.set(types);
    });
  }

  selectProperty(property: AlojamientoCard | null) {
    this.selectedPropertySignal.set(property);
    if (!property) {
      this.roomsSignal.set([]);
      this.photosSignal.set([]);
      return;
    }

    this.loadRooms(property.alojamientoId);
    this.loadPhotos(property.alojamientoId);
  }

  loadRooms(alojamientoId: number) {
    this.alojamientosService.getRoomsByProperty(alojamientoId).subscribe((rooms) => {
      this.roomsSignal.set(rooms);
    });
  }

  loadPhotos(alojamientoId: number) {
    this.alojamientosService.getPhotosByProperty(alojamientoId).subscribe((photos) => {
      this.photosSignal.set(photos.sort((left, right) => left.orden - right.orden));
    });
  }

  createProperty(form: AlojamientoForm, socioId = this.getCurrentSocioId(), onDone?: (property: AlojamientoCard) => void) {
    if (!socioId) {
      this.messageSignal.set('No se pudo identificar el socio autenticado.');
      return;
    }

    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.createPartnerProperty({ ...form, socioId }).subscribe({
      next: (property) => {
        this.messageSignal.set('Alojamiento creado correctamente.');
        this.savingSignal.set(false);
        this.loadProperties(socioId);
        this.selectProperty(property);
        onDone?.(property);
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo crear el alojamiento en el backend de produccion.');
      },
    });
  }

  updateProperty(
    alojamientoId: number,
    form: AlojamientoForm,
    socioId = this.getCurrentSocioId(),
    onDone?: () => void,
  ) {
    if (!socioId) {
      this.messageSignal.set('No se pudo identificar el socio autenticado.');
      return;
    }

    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.updatePartnerProperty(alojamientoId, { ...form, socioId }).subscribe({
      next: () => {
        this.messageSignal.set('Alojamiento actualizado correctamente.');
        this.savingSignal.set(false);
        this.loadProperties(socioId);
        onDone?.();
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo actualizar el alojamiento.');
      },
    });
  }

  deleteProperty(alojamientoId: number, socioId = this.getCurrentSocioId(), onDone?: () => void) {
    if (!socioId) {
      this.messageSignal.set('No se pudo identificar el socio autenticado.');
      return;
    }

    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.deletePartnerProperty(alojamientoId).subscribe({
      next: () => {
        this.messageSignal.set('Alojamiento eliminado correctamente.');
        this.savingSignal.set(false);
        if (this.selectedPropertySignal()?.alojamientoId === alojamientoId) {
          this.selectedPropertySignal.set(null);
          this.roomsSignal.set([]);
        }
        this.loadProperties(socioId);
        onDone?.();
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo eliminar el alojamiento.');
      },
    });
  }

  createRoom(form: HabitacionForm, onDone?: (room: Habitacion) => void) {
    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.createRoom(form).subscribe({
      next: (room) => {
        this.messageSignal.set('Habitacion creada correctamente.');
        this.savingSignal.set(false);
        this.loadRooms(form.alojamientoId);
        onDone?.(room);
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo crear la habitacion para este alojamiento.');
      },
    });
  }

  updateRoom(habitacionId: number, form: HabitacionForm, onDone?: () => void) {
    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.updateRoom(habitacionId, form).subscribe({
      next: () => {
        this.messageSignal.set('Habitacion actualizada correctamente.');
        this.savingSignal.set(false);
        this.loadRooms(form.alojamientoId);
        onDone?.();
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo actualizar la habitacion.');
      },
    });
  }

  deleteRoom(habitacionId: number, alojamientoId: number, onDone?: () => void) {
    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.deleteRoom(habitacionId).subscribe({
      next: () => {
        this.messageSignal.set('Habitacion eliminada correctamente.');
        this.savingSignal.set(false);
        this.loadRooms(alojamientoId);
        onDone?.();
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo eliminar la habitacion.');
      },
    });
  }

  uploadPhoto(form: FotoAlojamientoForm, onDone?: (photo: FotoAlojamiento) => void) {
    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.uploadPhotoViaCloudinary(form).subscribe({
      next: (photo) => {
        this.messageSignal.set('Imagen cargada correctamente desde Cloudinary.');
        this.savingSignal.set(false);
        this.loadPhotos(form.alojamientoId);
        this.loadProperties(this.selectedPropertySignal()?.socioId ?? this.getCurrentSocioId());
        onDone?.(photo);
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo cargar la imagen con Cloudinary.');
      },
    });
  }

  uploadPhotoFile(form: FotoAlojamientoForm, file: File, onDone?: (photo: FotoAlojamiento) => void) {
    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.uploadPhotoFileViaCloudinary(form, file).subscribe({
      next: (photo) => {
        this.messageSignal.set('Imagen cargada correctamente desde Cloudinary.');
        this.savingSignal.set(false);
        this.loadPhotos(form.alojamientoId);
        this.loadProperties(this.selectedPropertySignal()?.socioId ?? this.getCurrentSocioId());
        onDone?.(photo);
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo cargar la imagen con Cloudinary.');
      },
    });
  }

  deletePhoto(fotoId: number, alojamientoId: number, onDone?: () => void) {
    this.savingSignal.set(true);
    this.messageSignal.set('');

    this.alojamientosService.deletePhoto(fotoId).subscribe({
      next: () => {
        this.messageSignal.set('Imagen eliminada correctamente.');
        this.savingSignal.set(false);
        this.loadPhotos(alojamientoId);
        onDone?.();
      },
      error: () => {
        this.savingSignal.set(false);
        this.messageSignal.set('No se pudo eliminar la imagen.');
      },
    });
  }

  getDefaultSocioId() {
    return this.getCurrentSocioId() ?? 0;
  }

  private getCurrentSocioId() {
    const session = this.authService.session();
    return session?.role === 'socio' ? (session.usuarioId ?? null) : null;
  }
}
