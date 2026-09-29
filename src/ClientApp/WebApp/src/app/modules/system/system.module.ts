import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SysRoleIndexComponent } from 'src/app/modules/system/role/index/index';
import { SysRoleDetailComponent } from 'src/app/modules/system/role/detail/detail';
import { SysUserIndexComponent } from 'src/app/modules/system/user/index/index';
import { SysUserDetailComponent } from 'src/app/modules/system/user/detail/detail';
import { SysDataScopeIndexComponent } from 'src/app/modules/system/permission/index/index';
import { SysDataScopeDetailComponent } from 'src/app/modules/system/permission/detail/detail';
import { SystemLogIndexComponent } from 'src/app/modules/system/log/index/index';
import { SystemLogDetailComponent } from 'src/app/modules/system/log/detail/detail';

const routes: Routes = [
  { path: '', redirectTo: 'role', pathMatch: 'full' },
  { path: 'role', component: SysRoleIndexComponent },
  { path: 'role/:id/detail', component: SysRoleDetailComponent },
  { path: 'user', component: SysUserIndexComponent },
  { path: 'user/:id/detail', component: SysUserDetailComponent },
  { path: 'permission', component: SysDataScopeIndexComponent },
  { path: 'permission/:id/detail', component: SysDataScopeDetailComponent },
  { path: 'log', component: SystemLogIndexComponent },
  { path: 'log/:id/detail', component: SystemLogDetailComponent },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    SysRoleIndexComponent,
    SysRoleDetailComponent,
    SysUserIndexComponent,
    SysUserDetailComponent,
    SysDataScopeIndexComponent,
    SysDataScopeDetailComponent,
    SystemLogIndexComponent,
    SystemLogDetailComponent,
  ],
})
export class SystemModule {}
