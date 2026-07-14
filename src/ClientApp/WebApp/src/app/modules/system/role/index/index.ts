import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonListModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemRoleItemDto } from '../../../../services/admin/models/system-mod/system-role-item-dto.model';

@Component({
  selector: 'app-system-role-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemRoleIndexComponent {
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  readonly roles = signal<SystemRoleItemDto[]>([]);
  readonly loading = signal(false);
  name = '';

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.systemRole
      .list(this.name || null, null, 1, 50, null)
      .subscribe({
        next: (page) => {
          this.roles.set(page.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  remove(role: SystemRoleItemDto): void {
    if (role.isSystem || !confirm(`确定删除角色“${role.name}”吗？`)) return;
    this.client.systemRole.delete(role.id).subscribe(() => {
      this.snackBar.open('角色已删除', '关闭', { duration: 2500 });
      this.load();
    });
  }
}
