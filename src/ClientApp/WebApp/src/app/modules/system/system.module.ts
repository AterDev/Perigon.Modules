import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SystemRoleIndexComponent } from 'src/app/modules/system/role/index/index';
import { SystemRoleAddComponent } from 'src/app/modules/system/role/add/add';
import { SystemRoleEditComponent } from 'src/app/modules/system/role/edit/edit';
import { SystemRoleDetailComponent } from 'src/app/modules/system/role/detail/detail';
import { SystemUserIndexComponent } from 'src/app/modules/system/user/index/index';
import { SystemUserAddComponent } from 'src/app/modules/system/user/add/add';
import { SystemUserEditComponent } from 'src/app/modules/system/user/edit/edit';
import { SystemUserDetailComponent } from 'src/app/modules/system/user/detail/detail';
import { SystemPermissionIndexComponent } from 'src/app/modules/system/permission/index/index';
import { SystemPermissionAddComponent } from 'src/app/modules/system/permission/add/add';
import { SystemPermissionEditComponent } from 'src/app/modules/system/permission/edit/edit';
import { SystemPermissionDetailComponent } from 'src/app/modules/system/permission/detail/detail';
import { SystemLogIndexComponent } from 'src/app/modules/system/log/index/index';
import { SystemLogDetailComponent } from 'src/app/modules/system/log/detail/detail';

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
