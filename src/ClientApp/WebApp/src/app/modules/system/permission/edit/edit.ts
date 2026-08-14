import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { PermissionType } from 'src/app/services/admin/models/entity/permission-type.model';
import { SystemPermissionGroupItemDto } from 'src/app/services/admin/models/system-mod/system-permission-group-item-dto.model';

@Component({
  selector: 'app-system-permission-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemPermissionEditComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly id = this.route.snapshot.paramMap.get('id')!;
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
    this.client.systemPermission.getDetail(this.id).subscribe((value) =>
      this.form.patchValue({
        name: value.name,
        description: value.description ?? '',
        enable: value.enable,
        permissionType: value.permissionType,
        systemPermissionGroupId: value.group.id,
      }),
    );
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemPermission
      .update(this.id, this.form.getRawValue())
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.systemPermission.updateSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.router.navigate(['/system/permission', this.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
