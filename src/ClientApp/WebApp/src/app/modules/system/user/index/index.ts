import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemUserItemDto } from '../../../../services/admin/models/system-mod/system-user-item-dto.model';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../share/components/confirm-dialog/confirm-dialog.component';
import { TranslateService } from '@ngx-translate/core';
@Component({
  selector: 'app-system-user-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemUserIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
  readonly users = signal<SystemUserItemDto[]>([]);
  readonly loading = signal(false);
  userName = '';

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.systemUser
      .filter(this.userName || null, null, 1, 50, null)
      .subscribe({
        next: (page) => {
          this.users.set(page.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  remove(user: SystemUserItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant('common.confirmDelete'),
          content: `确定删除账号“${user.userName}”吗？`,
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.systemUser.delete(user.id).subscribe(() => {
          this.snackBar.open('账号已删除', '关闭', { duration: 2500 });
          this.load();
        });
      });
  }
}
