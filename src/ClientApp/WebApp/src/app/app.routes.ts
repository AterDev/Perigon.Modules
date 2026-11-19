import { Routes } from '@angular/router';
import { Login } from 'src/app/pages/login/login';
import { LayoutComponent } from 'src/app/layout/layout';
import { Notfound } from 'src/app/pages/notfound/notfound';
import { AuthGuard } from 'src/app/modules/share/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
      {
        path: 'system',
        loadChildren: () =>
          import('src/app/modules/system/system.module').then(
            (m) => m.SystemModule,
          ),
      },
      {
        path: 'resource',
        loadChildren: () =>
          import('src/app/modules/resource/resource.module').then(
            (m) => m.ResourceModule,
          ),
      },
      {
        path: 'cms',
        loadChildren: () =>
          import('src/app/modules/cms/cms.module').then((m) => m.CmsModule),
      },
      {
        path: 'audit',
        loadChildren: () =>
          import('src/app/modules/audit/audit.routes').then(
            (m) => m.auditRoutes,
          ),
      },
      {
        path: 'notification',
        loadChildren: () =>
          import('src/app/modules/notification/notification.routes').then(
            (m) => m.notificationRoutes,
          ),
      },
      // {
      //   path: 'sys-config',
      //   children: [
      //     { path: '', redirectTo: '/sys-config/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('src/app/pages/sys-config/index/index').then(m => m.Index) },
      //   ]
      // },
    ],
  },

  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', component: Notfound },
];
