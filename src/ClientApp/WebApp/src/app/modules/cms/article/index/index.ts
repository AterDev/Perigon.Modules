import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonListModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ArticleItemDto } from '../../../../services/admin/models/cmsmod/article-item-dto.model';
import { ArticleCategoryItemDto } from '../../../../services/admin/models/cmsmod/article-category-item-dto.model';

@Component({
  selector: 'app-article-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleIndexComponent {
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  readonly articles = signal<ArticleItemDto[]>([]);
  readonly categories = signal<ArticleCategoryItemDto[]>([]);
  readonly loading = signal(false);
  title = '';
  catalogId = '';
  constructor() {
    this.client.articleCategory
      .list(null, 1, 100, null)
      .subscribe((page) => this.categories.set(page.data));
    this.load();
  }
  load(): void {
    this.loading.set(true);
    this.client.article
      .list(
        this.title || null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        this.catalogId || null,
        null,
        1,
        50,
        null,
      )
      .subscribe({
        next: (page) => {
          this.articles.set(page.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }
  remove(item: ArticleItemDto): void {
    if (!confirm(`确定删除文章“${item.title}”吗？`)) return;
    this.client.article.delete(item.id).subscribe(() => {
      this.snackBar.open('文章已删除', '关闭', { duration: 2500 });
      this.load();
    });
  }
}
