import { Routes } from '@angular/router';
import { AdminGuard } from 'src/app/modules/share/admin.guard';
import { NotificationInboxComponent } from './inbox/inbox';
import { NotificationIndexComponent } from './index/index';

export const notificationRoutes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  {
    path: 'index',
    component: NotificationIndexComponent,
    canActivate: [AdminGuard],
  },
  { path: 'inbox', component: NotificationInboxComponent },
];
