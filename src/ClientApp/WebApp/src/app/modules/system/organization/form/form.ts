import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SysOrganization } from 'src/app/services/admin/models/entity/sys-organization.model';

export interface SysOrganizationOption {
  organization: SysOrganization;
  level: number;
}

export interface SysOrganizationFormData {
  organization?: SysOrganization;
  parentId?: string | null;
  options: SysOrganizationOption[];
}

@Component({
  selector: 'app-sys-organization-form',
  imports: CommonFormModules,
  templateUrl: './form.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysOrganizationFormComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(
    MatDialogRef<SysOrganizationFormComponent>,
  );
  private readonly data = inject<SysOrganizationFormData>(MAT_DIALOG_DATA);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly editing = !!this.data.organization;
  readonly options = this.data.options;
  readonly saving = signal(false);
  readonly form = this.fb.group({
    name: this.fb.nonNullable.control('', [
      Validators.required,
      Validators.maxLength(100),
    ]),
    externalId: this.fb.nonNullable.control('', Validators.maxLength(40)),
    parentId: this.fb.control<string | null>(null),
  });

  constructor() {
    const organization = this.data.organization;
    if (organization) {
      this.form.patchValue({
        name: organization.name,
        externalId: organization.externalId ?? '',
        parentId: organization.parentId ?? null,
      });
    } else if (this.data.parentId) {
      this.form.controls.parentId.setValue(this.data.parentId);
    }
  }

  save(): void {
    if (this.saving()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.form.getRawValue();
    const payload = {
      name: value.name,
      parentId: value.parentId,
      externalId: value.externalId || null,
    };
    const request = this.data.organization
      ? this.client.sysOrganization.update(this.data.organization.id, payload)
      : this.client.sysOrganization.add(payload);

    request.subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(
            this.editing
              ? this.i18nKeys.systemOrganization.updateSuccess
              : this.i18nKeys.systemOrganization.createSuccess,
          ),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2500 },
        );
        this.dialogRef.close({ saved: true });
      },
      error: () => this.saving.set(false),
    });
  }
}
