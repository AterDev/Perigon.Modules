import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { GenderType } from 'src/app/services/admin/models/perigon/gender-type.model';
import { SystemRoleItemDto } from 'src/app/services/admin/models/system-mod/system-role-item-dto.model';

@Component({
  selector: 'app-system-user-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemUserEditComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(MatDialogRef<SystemUserEditComponent>);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly id = this.data.id;
  readonly roles = signal<SystemRoleItemDto[]>([]);
  readonly genders = [
    { value: GenderType.Male, labelKey: I18N_KEYS.systemUser.genderTypes.male },
    { value: GenderType.Female, labelKey: I18N_KEYS.systemUser.genderTypes.female },
    { value: GenderType.Else, labelKey: I18N_KEYS.systemUser.genderTypes.else },
  ] as const;
  saving = false;
  readonly form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    password: [''],
    roleIds: [[] as string[]],
    realName: [''],
    email: ['', Validators.email],
    phoneNumber: [''],
    avatar: [''],
    sex: GenderType.Else,
  });

  constructor() {
    this.client.systemRole
      .list(null, null, 1, 100, null)
      .subscribe((page) => this.roles.set(page.data));
    this.client.systemUser.getDetail(this.id).subscribe((value) =>
      this.form.patchValue({
        userName: value.userName,
        realName: value.realName ?? '',
        email: value.email ?? '',
        phoneNumber: value.phoneNumber ?? '',
        avatar: value.avatar ?? '',
        sex: value.sex,
      }),
    );
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving = true;
    this.client.systemUser
      .update(this.id, { ...value, password: value.password || null })
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.systemUser.updateSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.dialogRef.close({ saved: true });
        },
        error: () => (this.saving = false),
      });
  }
}
