import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { LayoutComponent } from './layout/layout';
import { Notfound } from './pages/notfound/notfound';
import { AuthGuard } from './modules/share/auth.guard';

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
          import('./modules/system/system.module').then((m) => m.SystemModule),
      },
      {
        path: 'resource',
        loadChildren: () =>
          import('./modules/resource/resource.module').then(
            (m) => m.ResourceModule,
          ),
      },
      {
        path: 'cms',
        loadChildren: () =>
          import('./modules/cms/cms.module').then((m) => m.CmsModule),
      },
      // {
      //   path: 'system-config',
      //   children: [
      //     { path: '', redirectTo: '/system-config/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/system-config/index/index').then(m => m.Index) },
      //   ]
      // },
    ],
  },

  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', component: Notfound },
];
