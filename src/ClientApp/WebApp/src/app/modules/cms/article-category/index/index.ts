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
import { ArticleCategoryItemDto } from '../../../../services/admin/models/cmsmod/article-category-item-dto.model';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../share/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-article-category-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleCategoryIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);
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
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant('common.confirmDelete'),
          content: this.translate.instant('cms.category.deleteConfirm', {
            name: item.name,
          }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.articleCategory.delete(item.id).subscribe(() => {
          this.snackBar.open(
            this.translate.instant('cms.category.deleteSuccess'),
            this.translate.instant('common.close'),
            { duration: 2500 },
          );
          this.load();
        });
      });
  }
}
