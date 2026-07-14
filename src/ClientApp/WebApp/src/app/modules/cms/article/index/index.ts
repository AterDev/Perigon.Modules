import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ArticleItemDto } from '../../../../services/admin/models/cmsmod/article-item-dto.model';
import { ArticleCategoryItemDto } from '../../../../services/admin/models/cmsmod/article-category-item-dto.model';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-article-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
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
    if (
      !confirm(
        this.translate.instant('cms.article.deleteConfirm', {
          title: item.title,
        }),
      )
    )
      return;
    this.client.article.delete(item.id).subscribe(() => {
      this.snackBar.open(
        this.translate.instant('cms.article.deleteSuccess'),
        this.translate.instant('common.close'),
        { duration: 2500 },
      );
      this.load();
    });
  }
}
