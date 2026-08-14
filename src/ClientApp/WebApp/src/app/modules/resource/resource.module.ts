import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ResourceIndexComponent } from 'src/app/modules/resource/resource/index/index';
import { ResourceConfigIndexComponent } from 'src/app/modules/resource/config/index/index';
import { ResourceDefinitionIndexComponent } from 'src/app/modules/resource/definition/index/index';

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
