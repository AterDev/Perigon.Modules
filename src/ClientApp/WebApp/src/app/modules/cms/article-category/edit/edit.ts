import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-article-category-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleCategoryEditComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(MatDialogRef<ArticleCategoryEditComponent>);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly id = this.data.id;
  saving = false;
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
  });
  constructor() {
    this.client.articleCategory
      .detail(this.id)
      .subscribe((value) => this.form.patchValue({ name: value.name }));
  }
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.articleCategory
      .update(this.id, this.form.getRawValue())
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.cms.category.updateSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.dialogRef.close({ saved: true });
        },
        error: () => (this.saving = false),
      });
  }
}
