import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemPermissionGroupItemDto } from '../../../../services/admin/models/system-mod/system-permission-group-item-dto.model';
import { PermissionType } from '../../../../services/admin/models/entity/permission-type.model';
@Component({
  selector: 'app-system-permission-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemPermissionAddComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  readonly groups = signal<SystemPermissionGroupItemDto[]>([]);
  readonly types = Object.entries(PermissionType).filter(
    ([, value]) => typeof value === 'number',
  ) as [string, PermissionType][];
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
        this.snackBar.open('权限已创建', '关闭', { duration: 2500 });
        this.router.navigate(['/system/permission']);
      },
      error: () => (this.saving = false),
    });
  }
}
