import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ResourceIndexComponent } from './resource/index/index';
import { ResourceAddComponent } from './resource/add/add';
import { ResourceEditComponent } from './resource/edit/edit';
import { ResourceDetailComponent } from './resource/detail/detail';
import { ResourceConfigIndexComponent } from './config/index/index';

const routes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  { path: 'index', component: ResourceIndexComponent },
  { path: 'add', component: ResourceAddComponent },
  { path: ':id/edit', component: ResourceEditComponent },
  { path: ':id/detail', component: ResourceDetailComponent },
  { path: 'config', component: ResourceConfigIndexComponent },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    ResourceIndexComponent,
    ResourceAddComponent,
    ResourceEditComponent,
    ResourceDetailComponent,
    ResourceConfigIndexComponent,
  ],
})
export class ResourceModule {}
