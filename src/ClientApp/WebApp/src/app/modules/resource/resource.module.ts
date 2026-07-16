import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ResourceIndexComponent } from './resource/index/index';
import { ResourceConfigIndexComponent } from './config/index/index';
import { ResourceDefinitionIndexComponent } from './definition/index/index';

const routes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  { path: 'index', component: ResourceIndexComponent },
  { path: 'config', component: ResourceConfigIndexComponent },
  { path: 'definition', component: ResourceDefinitionIndexComponent },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    ResourceIndexComponent,
    ResourceConfigIndexComponent,
    ResourceDefinitionIndexComponent,
  ],
})
export class ResourceModule { }
