import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ArticleCategoryItemDto } from '../../../../services/admin/models/cmsmod/article-category-item-dto.model';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-article-category-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleCategoryAddComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly categories = signal<ArticleCategoryItemDto[]>([]);
  saving = false;
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    parentId: [''],
  });
  constructor() {
    this.client.articleCategory
      .list(null, 1, 100, null)
      .subscribe((page) => this.categories.set(page.data));
  }
  save(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    this.saving = true;
    this.client.articleCategory
      .add({ name: value.name, parentId: value.parentId || null })
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('cms.category.createSuccess'),
            this.translate.instant('common.close'),
            { duration: 2500 },
          );
          this.router.navigate(['/cms/article-category']);
        },
        error: () => (this.saving = false),
      });
  }
}
