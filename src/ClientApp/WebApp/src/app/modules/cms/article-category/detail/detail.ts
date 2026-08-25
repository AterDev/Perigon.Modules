import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ArticleCategoryDetailDto } from 'src/app/services/admin/models/cmsmod/article-category-detail-dto.model';
import { MatDialog } from '@angular/material/dialog';
import { ArticleCategoryEditComponent } from 'src/app/modules/cms/article-category/edit/edit';

@Component({
  selector: 'app-article-category-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleCategoryDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly category = signal<ArticleCategoryDetailDto | null>(null);
  constructor() {
    this.load();
  }

  load(): void {
    this.client.articleCategory
      .detail(this.id)
      .subscribe((value) => this.category.set(value));
  }

  edit(): void {
    this.dialog
      .open(ArticleCategoryEditComponent, {
        width: '520px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: this.id },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }
}
