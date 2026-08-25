import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SystemRoleIndexComponent } from 'src/app/modules/system/role/index/index';
import { SystemRoleDetailComponent } from 'src/app/modules/system/role/detail/detail';
import { SystemUserIndexComponent } from 'src/app/modules/system/user/index/index';
import { SystemUserDetailComponent } from 'src/app/modules/system/user/detail/detail';
import { SystemPermissionIndexComponent } from 'src/app/modules/system/permission/index/index';
import { SystemPermissionDetailComponent } from 'src/app/modules/system/permission/detail/detail';
import { SystemLogIndexComponent } from 'src/app/modules/system/log/index/index';
import { SystemLogDetailComponent } from 'src/app/modules/system/log/detail/detail';

const routes: Routes = [
  { path: '', redirectTo: 'role', pathMatch: 'full' },
  { path: 'role', component: SystemRoleIndexComponent },
  { path: 'role/:id/detail', component: SystemRoleDetailComponent },
  { path: 'user', component: SystemUserIndexComponent },
  { path: 'user/:id/detail', component: SystemUserDetailComponent },
  { path: 'permission', component: SystemPermissionIndexComponent },
  { path: 'permission/:id/detail', component: SystemPermissionDetailComponent },
  { path: 'log', component: SystemLogIndexComponent },
  { path: 'log/:id/detail', component: SystemLogDetailComponent },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    SystemRoleIndexComponent,
    SystemRoleDetailComponent,
    SystemUserIndexComponent,
    SystemUserDetailComponent,
    SystemPermissionIndexComponent,
    SystemPermissionDetailComponent,
    SystemLogIndexComponent,
    SystemLogDetailComponent,
  ],
})
export class SystemModule {}
