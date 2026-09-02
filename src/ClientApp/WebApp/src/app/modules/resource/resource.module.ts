import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ResourceIndexComponent } from 'src/app/modules/resource/resource/index/index';
import { ResourceConfigIndexComponent } from 'src/app/modules/resource/config/index/index';
import { ResourceDefinitionIndexComponent } from 'src/app/modules/resource/definition/index/index';
import { UserResourceIndexComponent } from 'src/app/modules/resource/personal-resource/index/index';
import { UserResourceReviewComponent } from 'src/app/modules/resource/personal-resource/review/index';
import { AdminGuard } from 'src/app/modules/share/admin.guard';

const routes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  { path: 'index', component: ResourceIndexComponent },
  { path: 'config', component: ResourceConfigIndexComponent },
  { path: 'definition', component: ResourceDefinitionIndexComponent },
  { path: 'mine', component: UserResourceIndexComponent },
  { path: 'review', component: UserResourceReviewComponent, canActivate: [AdminGuard] },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    ResourceIndexComponent,
    ResourceConfigIndexComponent,
    ResourceDefinitionIndexComponent,
    UserResourceIndexComponent,
    UserResourceReviewComponent,
  ],
})
export class ResourceModule { }
