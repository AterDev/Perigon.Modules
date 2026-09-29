import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { DataScopeType } from 'src/app/services/admin/models/entity/data-scope-type.model';
import { SysDataScopeGroupItemDto } from 'src/app/services/admin/models/system-mod/sys-data-scope-group-item-dto.model';

@Component({
  selector: 'app-sys-data-scope-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysDataScopeEditComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly scopeTypes = DataScopeType;
  readonly typeOptions = [
    { value: DataScopeType.None, label: I18N_KEYS.sysDataScope.types.none },
    { value: DataScopeType.All, label: I18N_KEYS.sysDataScope.types.all },
    { value: DataScopeType.Include, label: I18N_KEYS.sysDataScope.types.include },
  ];
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(MatDialogRef<SysDataScopeEditComponent>);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly id = this.data.id;
  readonly groups = signal<SysDataScopeGroupItemDto[]>([]);
  saving = false;
  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(60)]],
    resourceCode: ['', [Validators.required, Validators.maxLength(30)]],
    scopeType: DataScopeType.Include,
    targetIdsText: [''],
    groupId: ['', Validators.required],
  });

  constructor() {
    this.client.sysDataScopeGroup
      .filter({ pageIndex: 1, pageSize: 100 })
      .subscribe((page) => this.groups.set(page.data));
    this.client.sysDataScope.getDetail(this.id).subscribe((value) =>
      this.form.patchValue({
        name: value.name,
        resourceCode: value.resourceCode,
        scopeType: value.scopeType,
        targetIdsText: value.targetIds.join('\n'),
        groupId: value.groupId,
      }),
    );
  }

  get isIncludeScope(): boolean {
    return this.form.controls.scopeType.value === DataScopeType.Include;
  }

  get hasInvalidTargetIds(): boolean {
    return this.parseTargetIds().some((id) => !UUID_PATTERN.test(id));
  }

  save(): void {
    if (this.form.invalid || this.hasInvalidTargetIds) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving = true;
    this.client.sysDataScope
      .update(this.id, {
        name: value.name.trim(),
        resourceCode: value.resourceCode.trim(),
        scopeType: value.scopeType,
        targetIds: value.scopeType === DataScopeType.Include ? this.parseTargetIds() : [],
        groupId: value.groupId,
      })
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.sysDataScope.updateSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.dialogRef.close({ saved: true });
        },
        error: () => (this.saving = false),
      });
  }

  private parseTargetIds(): string[] {
    return [...new Set(this.form.controls.targetIdsText.value.split(/[\s,;]+/).filter(Boolean))];
  }
}

const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
