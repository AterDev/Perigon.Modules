import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ArticleCategoryItemDto } from 'src/app/services/admin/models/cmsmod/article-category-item-dto.model';
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
  private readonly dialogRef = inject(MatDialogRef<ArticleCategoryAddComponent>);
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving = true;
    this.client.articleCategory
      .add({ name: value.name, parentId: value.parentId || null })
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.cms.category.createSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.dialogRef.close({ saved: true });
        },
        error: () => (this.saving = false),
      });
  }
}
