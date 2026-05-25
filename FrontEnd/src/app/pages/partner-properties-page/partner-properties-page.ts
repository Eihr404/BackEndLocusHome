import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  AlojamientoCard,
  AlojamientoForm,
  FotoAlojamiento,
  FotoAlojamientoForm,
  Habitacion,
  HabitacionForm,
} from '../../models/alojamiento.model';
import { PartnerService } from '../../services/partner.service';

@Component({
  selector: 'app-partner-properties-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './partner-properties-page.html',
  styleUrl: './partner-properties-page.css',
})
export class PartnerPropertiesPageComponent {
  private readonly partnerService = inject(PartnerService);

  readonly properties = this.partnerService.properties;
  readonly selectedProperty = this.partnerService.selectedProperty;
  readonly rooms = this.partnerService.rooms;
  readonly photos = this.partnerService.photos;
  readonly saving = this.partnerService.saving;
  readonly loading = this.partnerService.loading;
  readonly message = this.partnerService.message;
  readonly tipoOptions = this.partnerService.tipoOptions;

  editingPropertyId: number | null = null;
  editingRoomId: number | null = null;

  propertyForm: AlojamientoForm = this.createEmptyPropertyForm();
  roomForm: HabitacionForm = this.createEmptyRoomForm();
  photoForm: FotoAlojamientoForm = this.createEmptyPhotoForm();

  constructor() {
    this.partnerService.loadPropertyTypes();
    this.partnerService.loadProperties(this.partnerService.getDefaultSocioId());
  }

  submitProperty() {
    if (this.editingPropertyId) {
      this.partnerService.updateProperty(this.editingPropertyId, this.propertyForm, undefined, () => {
        this.cancelPropertyEdit();
      });
      return;
    }

    this.partnerService.createProperty(this.propertyForm, undefined, (property) => {
      this.selectProperty(property);
      this.propertyForm = this.createEmptyPropertyForm();
    });
  }

  editProperty(property: AlojamientoCard) {
    this.editingPropertyId = property.alojamientoId;
    this.propertyForm = {
      socioId: property.socioId ?? this.partnerService.getDefaultSocioId(),
      tipoAlojamientoId: property.tipoAlojamientoId ?? 1,
      nombre: property.nombre,
      descripcion: property.descripcion ?? '',
      direccion: property.direccion,
      ciudad: property.ciudad,
      estrellas: property.estrellas ?? null,
      admiteMascotas: property.admiteMascotas ?? false,
      tienePiscina: property.tienePiscina ?? false,
      tieneParqueadero: property.tieneParqueadero ?? false,
    };
  }

  cancelPropertyEdit() {
    this.editingPropertyId = null;
    this.propertyForm = this.createEmptyPropertyForm();
  }

  deleteProperty(property: AlojamientoCard) {
    const confirmed = window.confirm(`Eliminar "${property.nombre}"? Esta accion tambien impacta sus habitaciones.`);
    if (!confirmed) {
      return;
    }

    this.partnerService.deleteProperty(property.alojamientoId, undefined, () => {
      if (this.editingPropertyId === property.alojamientoId) {
        this.cancelPropertyEdit();
      }
    });
  }

  selectProperty(property: AlojamientoCard) {
    this.partnerService.selectProperty(property);
    this.cancelRoomEdit();
    this.roomForm = {
      ...this.createEmptyRoomForm(),
      alojamientoId: property.alojamientoId,
    };
    this.photoForm = {
      ...this.createEmptyPhotoForm(),
      alojamientoId: property.alojamientoId,
    };
  }

  submitRoom() {
    if (!this.selectedProperty()) {
      return;
    }

    if (this.editingRoomId) {
      this.partnerService.updateRoom(this.editingRoomId, this.roomForm, () => {
        this.cancelRoomEdit();
      });
      return;
    }

    this.partnerService.createRoom(this.roomForm, () => {
      this.roomForm = {
        ...this.createEmptyRoomForm(),
        alojamientoId: this.selectedProperty()?.alojamientoId ?? 0,
      };
    });
  }

  editRoom(room: Habitacion) {
    this.editingRoomId = room.habitacionId;
    this.roomForm = {
      alojamientoId: room.alojamientoId,
      nombre: room.nombre,
      descripcion: room.descripcion ?? '',
      capacidadAdultos: room.capacidadAdultos,
      capacidadNinos: room.capacidadNinos,
      numBanos: room.numBanos ?? 1,
      numDormitorios: room.numDormitorios ?? 1,
      tieneCocina: room.tieneCocina ?? false,
      tieneAireAcondicionado: room.tieneAireAcondicionado ?? false,
      superficieM2: room.superficieM2 ?? null,
      precioNoche: room.precioNoche,
    };
  }

  cancelRoomEdit() {
    this.editingRoomId = null;
    this.roomForm = {
      ...this.createEmptyRoomForm(),
      alojamientoId: this.selectedProperty()?.alojamientoId ?? 0,
    };
  }

  deleteRoom(room: Habitacion) {
    const confirmed = window.confirm(`Eliminar la habitacion "${room.nombre}"?`);
    if (!confirmed) {
      return;
    }

    this.partnerService.deleteRoom(room.habitacionId, room.alojamientoId, () => {
      if (this.editingRoomId === room.habitacionId) {
        this.cancelRoomEdit();
      }
    });
  }

  submitPhoto() {
    const property = this.selectedProperty();
    if (!property) {
      return;
    }

    this.partnerService.uploadPhoto(
      {
        ...this.photoForm,
        alojamientoId: property.alojamientoId,
      },
      () => {
        this.photoForm = {
          ...this.createEmptyPhotoForm(),
          alojamientoId: property.alojamientoId,
        };
      },
    );
  }

  deletePhoto(photo: FotoAlojamiento) {
    const confirmed = window.confirm('Eliminar esta imagen del alojamiento?');
    if (!confirmed) {
      return;
    }

    this.partnerService.deletePhoto(photo.fotoId, photo.alojamientoId);
  }

  private createEmptyPropertyForm(): AlojamientoForm {
    return {
      socioId: this.partnerService.getDefaultSocioId(),
      tipoAlojamientoId: 1,
      nombre: '',
      descripcion: '',
      direccion: '',
      ciudad: '',
      estrellas: 3,
      admiteMascotas: false,
      tienePiscina: false,
      tieneParqueadero: false,
    };
  }

  private createEmptyRoomForm(): HabitacionForm {
    return {
      alojamientoId: this.selectedProperty()?.alojamientoId ?? 0,
      nombre: '',
      descripcion: '',
      capacidadAdultos: 2,
      capacidadNinos: 0,
      numBanos: 1,
      numDormitorios: 1,
      tieneCocina: false,
      tieneAireAcondicionado: false,
      superficieM2: null,
      precioNoche: 0,
    };
  }

  private createEmptyPhotoForm(): FotoAlojamientoForm {
    return {
      alojamientoId: this.selectedProperty()?.alojamientoId ?? 0,
      sourceUrl: '',
      orden: 0,
      descripcion: '',
    };
  }
}
