import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';

import { PartnerService } from '../../services/partner.service';

@Component({
  selector: 'app-partner-dashboard-page',
  imports: [CommonModule],
  templateUrl: './partner-dashboard-page.html',
  styleUrl: './partner-dashboard-page.css',
})
export class PartnerDashboardPageComponent {
  private readonly partnerService = inject(PartnerService);

  readonly metrics = this.partnerService.metrics;
  readonly properties = this.partnerService.properties;

  constructor() {
    this.partnerService.loadProperties(this.partnerService.getDefaultSocioId());
  }
}
