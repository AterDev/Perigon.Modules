import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';

@Component({
  selector: 'app-system-role-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemRoleEditComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(MatDialogRef<SystemRoleEditComponent>);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly id = this.data.id;
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    nameValue: ['', Validators.required],
    isSystem: false,
  });
  saving = false;

  constructor() {
    this.client.systemRole
      .detail(this.id)
      .subscribe((value) => this.form.patchValue(value));
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemRole.update(this.id, this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.systemRole.updateSuccess),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2500 },
        );
        this.dialogRef.close({ saved: true });
      },
      error: () => (this.saving = false),
    });
  }
}
