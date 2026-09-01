import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ResourceIndexComponent } from 'src/app/modules/resource/resource/index/index';
import { ResourceConfigIndexComponent } from 'src/app/modules/resource/config/index/index';
import { ResourceDefinitionIndexComponent } from 'src/app/modules/resource/definition/index/index';
import { PersonalResourceIndexComponent } from 'src/app/modules/resource/personal-resource/index/index';
import { PersonalResourceReviewComponent } from 'src/app/modules/resource/personal-resource/review/index';

const routes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  { path: 'index', component: ResourceIndexComponent },
  { path: 'config', component: ResourceConfigIndexComponent },
  { path: 'definition', component: ResourceDefinitionIndexComponent },
  { path: 'mine', component: PersonalResourceIndexComponent },
  { path: 'review', component: PersonalResourceReviewComponent },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    ResourceIndexComponent,
    ResourceConfigIndexComponent,
    ResourceDefinitionIndexComponent,
    PersonalResourceIndexComponent,
    PersonalResourceReviewComponent,
  ],
})
export class ResourceModule { }
