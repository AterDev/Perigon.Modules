import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonListModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ArticleCategoryItemDto } from '../../../../services/admin/models/cmsmod/article-category-item-dto.model';

@Component({
  selector: 'app-article-category-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleCategoryIndexComponent {
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  readonly categories = signal<ArticleCategoryItemDto[]>([]);
  name = '';
  constructor() {
    this.load();
  }
  load(): void {
    this.client.articleCategory
      .list(this.name || null, 1, 100, null)
      .subscribe((page) => this.categories.set(page.data));
  }
  remove(item: ArticleCategoryItemDto): void {
    if (!confirm(`确定删除分类“${item.name}”吗？`)) return;
    this.client.articleCategory.delete(item.id).subscribe(() => {
      this.snackBar.open('分类已删除', '关闭', { duration: 2500 });
      this.load();
    });
  }
}
