import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, map, of } from 'rxjs';

import {
  AlojamientoCard,
  AlojamientoForm,
  FotoAlojamiento,
  FotoAlojamientoForm,
  Habitacion,
  HabitacionForm,
  TipoAlojamientoOption,
} from '../models/alojamiento.model';
import { ApiEnvelope } from '../models/api-response.model';
import { ALOJAMIENTOS_API_BASE_URL, PARTNER_API_BASE_URL } from './api.config';

const MOCK_ALOJAMIENTOS: AlojamientoCard[] = [
  {
    alojamientoId: 1,
    nombre: 'Casa Nube Andina',
    tipoAlojamiento: 'Casa',
    ciudad: 'Quito',
    direccion: 'La Floresta, Quito',
    precioNocheMinimo: 74,
    moneda: 'USD',
    estrellas: 4,
    disponible: true,
    tienePiscina: false,
    admiteMascotas: true,
    descripcion: 'Vista urbana, cocina equipada y check-in autonomo.',
  },
  {
    alojamientoId: 2,
    nombre: 'Hotel Puerto Arena',
    tipoAlojamiento: 'Hotel',
    ciudad: 'Manta',
    direccion: 'Malecon Escenico',
    precioNocheMinimo: 118,
    moneda: 'USD',
    estrellas: 5,
    disponible: true,
    tienePiscina: true,
    admiteMascotas: false,
    descripcion: 'Frente al mar con habitaciones premium y rooftop.',
  },
  {
    alojamientoId: 3,
    nombre: 'Loft Centro Historico',
    tipoAlojamiento: 'Suite',
    ciudad: 'Cuenca',
    direccion: 'Calle Larga',
    precioNocheMinimo: 59,
    moneda: 'USD',
    estrellas: 4,
    disponible: true,
    tienePiscina: false,
    admiteMascotas: false,
    descripcion: 'Ideal para estancias cortas y turismo cultural.',
  },
];

const MOCK_HABITACIONES: Habitacion[] = [
  {
    habitacionId: 101,
    alojamientoId: 1,
    nombre: 'Suite Jardin',
    descripcion: 'Suite base para demostracion.',
    capacidadAdultos: 2,
    capacidadNinos: 1,
    numBanos: 1,
    numDormitorios: 1,
    precioNoche: 74,
    estado: 'Disponible',
  },
  {
    habitacionId: 201,
    alojamientoId: 2,
    nombre: 'Ocean Deluxe',
    descripcion: 'Habitacion con vista al mar.',
    capacidadAdultos: 3,
    capacidadNinos: 2,
    numBanos: 2,
    numDormitorios: 1,
    precioNoche: 118,
    estado: 'Disponible',
  },
  {
    habitacionId: 301,
    alojamientoId: 3,
    nombre: 'Loft Premium',
    descripcion: 'Unidad compacta con cocina.',
    capacidadAdultos: 2,
    capacidadNinos: 0,
    numBanos: 1,
    numDormitorios: 1,
    precioNoche: 59,
    estado: 'Disponible',
  },
];

@Injectable({ providedIn: 'root' })
export class AlojamientosService {
  private readonly http = inject(HttpClient);

  getCatalog(filters?: { city?: string; search?: string }) {
    let params = new HttpParams();

    if (filters?.city) {
      params = params.set('ciudad', filters.city);
    }

    if (filters?.search) {
      params = params.set('search', filters.search);
    }

    return this.http
      .get<ApiEnvelope<AlojamientoCard[]> | AlojamientoCard[]>(`${ALOJAMIENTOS_API_BASE_URL}/alojamientos`, {
        params,
      })
      .pipe(
        map((response) => this.unwrapList(response).map((item) => this.normalizeProperty(item))),
        map((items) => this.applyLocalFilters(items, filters)),
        catchError(() => of(this.applyLocalFilters(MOCK_ALOJAMIENTOS, filters))),
      );
  }

  getById(id: number) {
    return this.http
      .get<ApiEnvelope<AlojamientoCard> | AlojamientoCard>(`${ALOJAMIENTOS_API_BASE_URL}/alojamientos/${id}`)
      .pipe(
        map((response) => {
          const item = this.unwrapItem(response);
          return item ? this.normalizeProperty(item) : null;
        }),
        catchError(() => of(MOCK_ALOJAMIENTOS.find((item) => item.alojamientoId === id) ?? null)),
      );
  }

  getRoomsByProperty(alojamientoId: number) {
    return this.http
      .get<unknown>(`${ALOJAMIENTOS_API_BASE_URL}/alojamientos/${alojamientoId}/habitaciones`)
      .pipe(
        map((response) => this.unwrapCollection(response).map((room) => this.normalizeRoom(room))),
        catchError(() =>
          of(MOCK_HABITACIONES.filter((room) => room.alojamientoId === alojamientoId)),
        ),
      );
  }

  getPartnerProperties(socioId: number) {
    return this.http.get<unknown>(`${PARTNER_API_BASE_URL}/alojamientos`).pipe(
      map((response) =>
        this.unwrapCollection(response)
          .map((item) => this.normalizeProperty(item))
          .filter((item) => item.socioId === socioId),
      ),
      catchError(() =>
        of(MOCK_ALOJAMIENTOS.filter((item) => (item.socioId ?? socioId) === socioId)),
      ),
    );
  }

  createPartnerProperty(form: AlojamientoForm) {
    const payload = {
      socioId: form.socioId,
      tipoAlojamientoId: form.tipoAlojamientoId,
      nombre: form.nombre,
      ciudad: form.ciudad,
      direccion: form.direccion,
      descripcion: form.descripcion,
      admiteMascotas: form.admiteMascotas,
      tienePiscina: form.tienePiscina,
      tieneParqueadero: form.tieneParqueadero,
    };

    return this.http
      .post<unknown>(`${PARTNER_API_BASE_URL}/alojamientos`, payload)
      .pipe(map((item) => this.normalizeProperty(item)));
  }

  updatePartnerProperty(alojamientoId: number, form: AlojamientoForm) {
    const payload = {
      nombre: form.nombre,
      ciudad: form.ciudad,
      direccion: form.direccion,
      descripcion: form.descripcion,
      tipoAlojamientoId: form.tipoAlojamientoId,
      admiteMascotas: form.admiteMascotas,
      tienePiscina: form.tienePiscina,
      tieneParqueadero: form.tieneParqueadero,
      estrellas: form.estrellas ?? null,
    };

    return this.http.put<void>(`${PARTNER_API_BASE_URL}/alojamientos/${alojamientoId}`, payload);
  }

  deletePartnerProperty(alojamientoId: number) {
    return this.http.delete<void>(`${PARTNER_API_BASE_URL}/alojamientos/${alojamientoId}`);
  }

  createRoom(form: HabitacionForm) {
    const payload = {
      alojamientoId: form.alojamientoId,
      nombre: form.nombre,
      descripcion: form.descripcion,
      capacidadAdultos: form.capacidadAdultos,
      capacidadNinos: form.capacidadNinos,
      numBanos: form.numBanos,
      numDormitorios: form.numDormitorios,
      tieneCocina: form.tieneCocina,
      tieneAireAcondicionado: form.tieneAireAcondicionado,
      superficieM2: form.superficieM2 ?? null,
      precioNoche: form.precioNoche,
    };

    return this.http
      .post<unknown>(`${PARTNER_API_BASE_URL}/alojamientos/${form.alojamientoId}/habitaciones`, payload)
      .pipe(map((room) => this.normalizeRoom(room)));
  }

  updateRoom(habitacionId: number, form: HabitacionForm) {
    const payload = {
      nombre: form.nombre,
      descripcion: form.descripcion,
      capacidadAdultos: form.capacidadAdultos,
      capacidadNinos: form.capacidadNinos,
      numBanos: form.numBanos,
      numDormitorios: form.numDormitorios,
      tieneCocina: form.tieneCocina,
      tieneAireAcondicionado: form.tieneAireAcondicionado,
      superficieM2: form.superficieM2 ?? null,
      precioNoche: form.precioNoche,
    };

    return this.http.put<void>(`${PARTNER_API_BASE_URL}/habitaciones/${habitacionId}`, payload);
  }

  deleteRoom(habitacionId: number) {
    return this.http.delete<void>(`${PARTNER_API_BASE_URL}/habitaciones/${habitacionId}`);
  }

  getPhotosByProperty(alojamientoId: number) {
    return this.http
      .get<unknown>(`${ALOJAMIENTOS_API_BASE_URL}/Fotos/alojamiento/${alojamientoId}`)
      .pipe(
        map((response) => this.unwrapCollection(response).map((photo) => this.normalizePhoto(photo))),
        catchError(() => of<FotoAlojamiento[]>([])),
      );
  }

  uploadPhotoViaCloudinary(form: FotoAlojamientoForm) {
    const payload = {
      alojamientoId: form.alojamientoId,
      sourceUrl: form.sourceUrl,
      orden: form.orden,
      descripcion: form.descripcion || null,
    };

    return this.http
      .post<unknown>(`${ALOJAMIENTOS_API_BASE_URL}/Fotos/cloudinary`, payload)
      .pipe(map((photo) => this.normalizePhoto(photo)));
  }

  deletePhoto(fotoId: number) {
    return this.http.delete<void>(`${ALOJAMIENTOS_API_BASE_URL}/Fotos/${fotoId}`);
  }

  getPropertyTypes() {
    return this.http.get<unknown>(`${ALOJAMIENTOS_API_BASE_URL}/Alojamientos/tipos`).pipe(
      map((response) =>
        this.unwrapCollection(response).map((item: any) => ({
          id: item.tipoAlojamientoId ?? item.TipoAlojamientoId ?? 0,
          nombre: item.nombre ?? item.Nombre ?? 'Tipo',
        })) satisfies TipoAlojamientoOption[],
      ),
      catchError(() =>
        this.http.get<unknown>(`${ALOJAMIENTOS_API_BASE_URL}/Alojamientos`).pipe(
          map((response) => this.extractTypesFromProperties(this.unwrapCollection(response))),
          catchError(() =>
            of<TipoAlojamientoOption[]>([
              { id: 1, nombre: 'Hotel' },
              { id: 2, nombre: 'Suite' },
              { id: 3, nombre: 'Hostal' },
              { id: 4, nombre: 'Casa' },
              { id: 5, nombre: 'Departamento' },
            ]),
          ),
        ),
      ),
    );
  }

  private unwrapList(response: ApiEnvelope<AlojamientoCard[]> | AlojamientoCard[]) {
    return this.unwrapCollection(response);
  }

  private unwrapItem(response: ApiEnvelope<AlojamientoCard> | AlojamientoCard) {
    if ('alojamientoId' in response) {
      return response;
    }

    return response.data ?? null;
  }

  private unwrapCollection(response: unknown) {
    if (Array.isArray(response)) {
      return response;
    }

    if (!response || typeof response !== 'object') {
      return [];
    }

    const collection = response as { data?: unknown; value?: unknown; items?: unknown };

    if (Array.isArray(collection.data)) {
      return collection.data;
    }

    if (Array.isArray(collection.value)) {
      return collection.value;
    }

    if (Array.isArray(collection.items)) {
      return collection.items;
    }

    return [];
  }

  private applyLocalFilters(items: AlojamientoCard[], filters?: { city?: string; search?: string }) {
    return items.filter((item) => {
      const matchCity = filters?.city
        ? item.ciudad.toLowerCase().includes(filters.city.toLowerCase())
        : true;
      const matchSearch = filters?.search
        ? `${item.nombre} ${item.descripcion ?? ''}`
            .toLowerCase()
            .includes(filters.search.toLowerCase())
        : true;

      return matchCity && matchSearch;
    });
  }

  private normalizeProperty(item: any): AlojamientoCard {
    const statusValue = String(item.estado ?? item.Estado ?? 'activo').toLowerCase();

    return {
      alojamientoId: item.alojamientoId ?? item.AlojamientoId ?? 0,
      socioId: item.socioId ?? item.SocioId,
      tipoAlojamientoId: item.tipoAlojamientoId ?? item.TipoAlojamientoId,
      nombre: item.nombre ?? item.Nombre ?? 'Alojamiento',
      tipoAlojamiento:
        item.tipoAlojamiento ??
        item.tipoAlojamientoNombre ??
        item.TipoAlojamientoNombre ??
        `Tipo ${item.tipoAlojamientoId ?? item.TipoAlojamientoId ?? ''}`.trim(),
      ciudad: item.ciudad ?? item.Ciudad ?? '',
      direccion: item.direccion ?? item.Direccion ?? '',
      descripcion: item.descripcion ?? item.Descripcion ?? '',
      precioNocheMinimo:
        Number(item.precioNocheMinimo ?? item.PrecioNocheMinimo ?? item.precioBase ?? 0) || 0,
      moneda: item.moneda ?? item.Moneda ?? 'USD',
      estrellas: Number(item.estrellas ?? item.Estrellas ?? 0) || 0,
      admiteMascotas: item.admiteMascotas ?? item.AdmiteMascotas ?? false,
      tienePiscina: item.tienePiscina ?? item.TienePiscina ?? false,
      tieneParqueadero: item.tieneParqueadero ?? item.TieneParqueadero ?? false,
      disponible: item.disponible ?? statusValue !== 'inactivo',
      estado: item.estado ?? item.Estado,
    };
  }

  private normalizeRoom(room: any): Habitacion {
    return {
      habitacionId: room.habitacionId ?? room.HabitacionId ?? 0,
      alojamientoId: room.alojamientoId ?? room.AlojamientoId ?? 0,
      nombre: room.nombre ?? room.Nombre ?? 'Habitacion',
      descripcion: room.descripcion ?? room.Descripcion ?? '',
      capacidadAdultos: room.capacidadAdultos ?? room.CapacidadAdultos ?? 0,
      capacidadNinos: room.capacidadNinos ?? room.CapacidadNinos ?? 0,
      numBanos: room.numBanos ?? room.NumBanos ?? 0,
      numDormitorios: room.numDormitorios ?? room.NumDormitorios ?? 0,
      tieneCocina: room.tieneCocina ?? room.TieneCocina ?? false,
      tieneAireAcondicionado:
        room.tieneAireAcondicionado ?? room.TieneAireAcondicionado ?? false,
      superficieM2: room.superficieM2 ?? room.SuperficieM2 ?? null,
      precioNoche: Number(room.precioNoche ?? room.PrecioNoche ?? room.precioPorNoche ?? 0) || 0,
      estado: room.estado ?? room.Estado ?? 'Disponible',
    };
  }

  private normalizePhoto(photo: any): FotoAlojamiento {
    return {
      fotoId: photo.fotoId ?? photo.FotoId ?? 0,
      alojamientoId: photo.alojamientoId ?? photo.AlojamientoId ?? 0,
      url: photo.url ?? photo.Url ?? '',
      orden: Number(photo.orden ?? photo.Orden ?? 0) || 0,
      descripcion: photo.descripcion ?? photo.Descripcion ?? null,
    };
  }

  private extractTypesFromProperties(items: unknown[]) {
    const mapById = new Map<number, TipoAlojamientoOption>();

    items
      .map((item) => this.normalizeProperty(item))
      .forEach((property) => {
        const id = property.tipoAlojamientoId ?? 0;
        const nombre = property.tipoAlojamiento || 'Tipo';
        if (!mapById.has(id)) {
          mapById.set(id, { id, nombre });
        }
      });

    return [...mapById.values()].filter((item) => item.id > 0);
  }
}
