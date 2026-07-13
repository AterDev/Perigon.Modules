import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { LayoutComponent } from './layout/layout';
import { Notfound } from './pages/notfound/notfound';
import { AuthGuard } from './share/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
      {
        path: 'system-role',
        children: [
          { path: '', redirectTo: '/system-role/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-role/index/index').then(m => m.Index) },
        ]
      },
      {
        path: 'system-user',
        children: [
          { path: '', redirectTo: '/system-user/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-user/index/index').then(m => m.Index) },
        ]
      },
      {
        path: 'system-logs',
        children: [
          { path: '', redirectTo: '/system-logs/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-logs/index/index').then(m => m.Index) },
        ]
      },
      {
        path: 'resource',
        children: [
          { path: '', redirectTo: '/resource/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/resource/index/index').then(m => m.ResourceIndex) },
        ]
      },
      {
        path: 'resource-config',
        children: [
          { path: '', redirectTo: '/resource-config/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/resource-config/index/index').then(m => m.ResourceConfigIndex) },
        ]
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
