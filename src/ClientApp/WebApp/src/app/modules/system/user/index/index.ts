import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SysUserItemDto } from 'src/app/services/admin/models/system-mod/sys-user-item-dto.model';
import { SysUserAddComponent } from 'src/app/modules/system/user/add/add';
import { SysUserEditComponent } from 'src/app/modules/system/user/edit/edit';

@Component({
  selector: 'app-sys-user-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysUserIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
  readonly users = signal<SysUserItemDto[]>([]);
  readonly loading = signal(false);
  userName = '';

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.sysUser
      .filter(this.userName || null, null, 1, 50, null)
      .subscribe({
        next: (page) => {
          this.users.set(page.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  add(): void {
    this.dialog
      .open(SysUserAddComponent, {
        width: '640px',
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  edit(user: SysUserItemDto): void {
    this.dialog
      .open(SysUserEditComponent, {
        width: '640px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: user.id },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  remove(user: SysUserItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.common.confirmDelete),
          content: this.translate.instant(this.i18nKeys.systemUser.deleteConfirm, {
            name: user.userName,
          }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.sysUser.delete(user.id).subscribe(() => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.systemUser.deleteSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.load();
        });
      });
  }
}
