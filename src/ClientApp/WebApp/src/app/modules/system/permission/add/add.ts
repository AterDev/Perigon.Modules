import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MatDialogRef } from '@angular/material/dialog';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { PermissionType } from 'src/app/services/admin/models/entity/permission-type.model';
import { SystemPermissionGroupItemDto } from 'src/app/services/admin/models/system-mod/system-permission-group-item-dto.model';

@Component({
  selector: 'app-system-permission-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemPermissionAddComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(MatDialogRef<SystemPermissionAddComponent>);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly groups = signal<SystemPermissionGroupItemDto[]>([]);
  readonly types = [
    { value: PermissionType.None, labelKey: I18N_KEYS.systemPermission.types.none },
    { value: PermissionType.Read, labelKey: I18N_KEYS.systemPermission.types.read },
    { value: PermissionType.Audit, labelKey: I18N_KEYS.systemPermission.types.audit },
    { value: PermissionType.Add, labelKey: I18N_KEYS.systemPermission.types.add },
    { value: PermissionType.Edit, labelKey: I18N_KEYS.systemPermission.types.edit },
    { value: PermissionType.Write, labelKey: I18N_KEYS.systemPermission.types.write },
    { value: PermissionType.AuditWrite, labelKey: I18N_KEYS.systemPermission.types.auditWrite },
  ] as const;
  saving = false;
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    enable: true,
    permissionType: PermissionType.Read,
    systemPermissionGroupId: ['', Validators.required],
  });

  constructor() {
    this.client.systemPermissionGroup
      .filter({ pageIndex: 1, pageSize: 100 })
      .subscribe((page) => this.groups.set(page.data));
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemPermission.add(this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.systemPermission.createSuccess),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2500 },
        );
        this.dialogRef.close({ saved: true });
      },
      error: () => (this.saving = false),
    });
  }
}
