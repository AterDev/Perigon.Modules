import { Routes } from '@angular/router';
import { AdminGuard } from 'src/app/modules/share/admin.guard';
import { AuditFlowIndexComponent } from './flow/index';
import { AuditRecordIndexComponent } from './record/index';
import { AuditTaskIndexComponent } from './task/index';

export const auditRoutes: Routes = [
  { path: '', redirectTo: 'tasks', pathMatch: 'full' },
  { path: 'tasks', component: AuditTaskIndexComponent },
  {
    path: 'records',
    component: AuditRecordIndexComponent,
    canActivate: [AdminGuard],
    data: { mine: false },
  },
  { path: 'mine', component: AuditRecordIndexComponent, data: { mine: true } },
  {
    path: 'flows',
    component: AuditFlowIndexComponent,
    canActivate: [AdminGuard],
  },
];
