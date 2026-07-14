import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SystemRoleIndexComponent } from './role/index/index';
import { SystemRoleAddComponent } from './role/add/add';
import { SystemRoleEditComponent } from './role/edit/edit';
import { SystemRoleDetailComponent } from './role/detail/detail';
import { SystemUserIndexComponent } from './user/index/index';
import { SystemUserAddComponent } from './user/add/add';
import { SystemUserEditComponent } from './user/edit/edit';
import { SystemUserDetailComponent } from './user/detail/detail';
import { SystemPermissionIndexComponent } from './permission/index/index';
import { SystemPermissionAddComponent } from './permission/add/add';
import { SystemPermissionEditComponent } from './permission/edit/edit';
import { SystemPermissionDetailComponent } from './permission/detail/detail';
import { SystemLogIndexComponent } from './log/index/index';
import { SystemLogDetailComponent } from './log/detail/detail';

const routes: Routes = [
  { path: '', redirectTo: 'role', pathMatch: 'full' },
  { path: 'role', component: SystemRoleIndexComponent },
  { path: 'role/add', component: SystemRoleAddComponent },
  { path: 'role/:id/edit', component: SystemRoleEditComponent },
  { path: 'role/:id/detail', component: SystemRoleDetailComponent },
  { path: 'user', component: SystemUserIndexComponent },
  { path: 'user/add', component: SystemUserAddComponent },
  { path: 'user/:id/edit', component: SystemUserEditComponent },
  { path: 'user/:id/detail', component: SystemUserDetailComponent },
  { path: 'permission', component: SystemPermissionIndexComponent },
  { path: 'permission/add', component: SystemPermissionAddComponent },
  { path: 'permission/:id/edit', component: SystemPermissionEditComponent },
  { path: 'permission/:id/detail', component: SystemPermissionDetailComponent },
  { path: 'log', component: SystemLogIndexComponent },
  { path: 'log/:id/detail', component: SystemLogDetailComponent },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    SystemRoleIndexComponent,
    SystemRoleAddComponent,
    SystemRoleEditComponent,
    SystemRoleDetailComponent,
    SystemUserIndexComponent,
    SystemUserAddComponent,
    SystemUserEditComponent,
    SystemUserDetailComponent,
    SystemPermissionIndexComponent,
    SystemPermissionAddComponent,
    SystemPermissionEditComponent,
    SystemPermissionDetailComponent,
    SystemLogIndexComponent,
    SystemLogDetailComponent,
  ],
})
export class SystemModule {}
