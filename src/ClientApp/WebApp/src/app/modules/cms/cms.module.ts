import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ArticleIndexComponent } from './article/index/index';
import { ArticleAddComponent } from './article/add/add';
import { ArticleEditComponent } from './article/edit/edit';
import { ArticleDetailComponent } from './article/detail/detail';
import { ArticleCategoryIndexComponent } from './article-category/index/index';
import { ArticleCategoryAddComponent } from './article-category/add/add';
import { ArticleCategoryEditComponent } from './article-category/edit/edit';
import { ArticleCategoryDetailComponent } from './article-category/detail/detail';

const routes: Routes = [
  { path: '', redirectTo: 'article', pathMatch: 'full' },
  { path: 'article', component: ArticleIndexComponent },
  { path: 'article/add', component: ArticleAddComponent },
  { path: 'article/:id/edit', component: ArticleEditComponent },
  { path: 'article/:id/detail', component: ArticleDetailComponent },
  { path: 'article-category', component: ArticleCategoryIndexComponent },
  { path: 'article-category/add', component: ArticleCategoryAddComponent },
  {
    path: 'article-category/:id/edit',
    component: ArticleCategoryEditComponent,
  },
  {
    path: 'article-category/:id/detail',
    component: ArticleCategoryDetailComponent,
  },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    ArticleIndexComponent,
    ArticleAddComponent,
    ArticleEditComponent,
    ArticleDetailComponent,
    ArticleCategoryIndexComponent,
    ArticleCategoryAddComponent,
    ArticleCategoryEditComponent,
    ArticleCategoryDetailComponent,
  ],
})
export class CmsModule {}
