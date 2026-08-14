import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { GenderType } from 'src/app/services/admin/models/perigon/gender-type.model';
import { SystemRoleItemDto } from 'src/app/services/admin/models/system-mod/system-role-item-dto.model';

@Component({
  selector: 'app-system-user-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemUserAddComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly roles = signal<SystemRoleItemDto[]>([]);
  readonly genders = [
    { value: GenderType.Male, labelKey: I18N_KEYS.systemUser.genderTypes.male },
    { value: GenderType.Female, labelKey: I18N_KEYS.systemUser.genderTypes.female },
    { value: GenderType.Else, labelKey: I18N_KEYS.systemUser.genderTypes.else },
  ] as const;
  saving = false;
  readonly form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(6)]],
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
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemUser.add(this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.systemUser.createSuccess),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2500 },
        );
        this.router.navigate(['/system/user']);
      },
      error: () => (this.saving = false),
    });
  }
}
