import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogRef } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';

@Component({
  selector: 'app-system-role-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemRoleAddComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(MatDialogRef<SystemRoleAddComponent>);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  saving = false;
  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    nameValue: ['', [Validators.required, Validators.maxLength(50)]],
    isSystem: false,
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemRole.add(this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.systemRole.createSuccess),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2500 },
        );
        this.dialogRef.close({ saved: true });
      },
      error: () => (this.saving = false),
    });
  }
}
