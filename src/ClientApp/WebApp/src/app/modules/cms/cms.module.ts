import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ArticleIndexComponent } from 'src/app/modules/cms/article/index/index';
import { ArticleAddComponent } from 'src/app/modules/cms/article/add/add';
import { ArticleEditComponent } from 'src/app/modules/cms/article/edit/edit';
import { ArticleDetailComponent } from 'src/app/modules/cms/article/detail/detail';
import { ArticleCategoryIndexComponent } from 'src/app/modules/cms/article-category/index/index';
import { ArticleCategoryDetailComponent } from 'src/app/modules/cms/article-category/detail/detail';

const routes: Routes = [
  { path: '', redirectTo: 'article', pathMatch: 'full' },
  { path: 'article', component: ArticleIndexComponent },
  { path: 'article/add', component: ArticleAddComponent },
  { path: 'article/:id/edit', component: ArticleEditComponent },
  { path: 'article/:id/detail', component: ArticleDetailComponent },
  { path: 'article-category', component: ArticleCategoryIndexComponent },
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
    ArticleCategoryDetailComponent,
  ],
})
export class CmsModule {}
