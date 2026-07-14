import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemPermissionItemDto } from '../../../../services/admin/models/system-mod/system-permission-item-dto.model';
@Component({
  selector: 'app-system-permission-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemPermissionIndexComponent {
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  readonly permissions = signal<SystemPermissionItemDto[]>([]);
  readonly loading = signal(false);
  name = '';

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.systemPermission
      .filter({ name: this.name || null, pageIndex: 1, pageSize: 100 })
      .subscribe({
        next: (page) => {
          this.permissions.set(page.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  remove(item: SystemPermissionItemDto): void {
    if (!confirm(`确定删除权限“${item.name}”吗？`)) return;
    this.client.systemPermission.delete(item.id).subscribe(() => {
      this.snackBar.open('权限已删除', '关闭', { duration: 2500 });
      this.load();
    });
  }
}
