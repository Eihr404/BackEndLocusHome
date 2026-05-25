import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';
import { PartnerLayoutComponent } from './layouts/partner-layout/partner-layout';
import { PublicLayoutComponent } from './layouts/public-layout/public-layout';
import { CatalogPageComponent } from './pages/catalog-page/catalog-page';
import { HomePageComponent } from './pages/home-page/home-page';
import { LoginPageComponent } from './pages/login-page/login-page';
import { PartnerDashboardPageComponent } from './pages/partner-dashboard-page/partner-dashboard-page';
import { PartnerPropertiesPageComponent } from './pages/partner-properties-page/partner-properties-page';
import { PartnerReservationsPageComponent } from './pages/partner-reservations-page/partner-reservations-page';
import { PropertyDetailPageComponent } from './pages/property-detail-page/property-detail-page';
import { RegisterPageComponent } from './pages/register-page/register-page';
import { ReservationsPageComponent } from './pages/reservations-page/reservations-page';

export const routes: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      { path: '', component: HomePageComponent, pathMatch: 'full' },
      { path: 'explorar', component: CatalogPageComponent },
      { path: 'alojamientos/:id', component: PropertyDetailPageComponent },
      { path: 'login', component: LoginPageComponent },
      { path: 'registro', component: RegisterPageComponent },
      {
        path: 'mis-reservas',
        component: ReservationsPageComponent,
        canActivate: [authGuard],
      },
    ],
  },
  {
    path: 'socio',
    component: PartnerLayoutComponent,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['socio'] },
    children: [
      { path: '', component: PartnerDashboardPageComponent, pathMatch: 'full' },
      { path: 'propiedades', component: PartnerPropertiesPageComponent },
      { path: 'reservas', component: PartnerReservationsPageComponent },
    ],
  },
  { path: '**', redirectTo: '' },
];
