import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SysRoleItemDto } from 'src/app/services/admin/models/system-mod/sys-role-item-dto.model';
import { SysRoleAddComponent } from 'src/app/modules/system/role/add/add';
import { SysRoleEditComponent } from 'src/app/modules/system/role/edit/edit';

@Component({
  selector: 'app-sys-role-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysRoleIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
  readonly roles = signal<SysRoleItemDto[]>([]);
  readonly loading = signal(false);
  name = '';

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.sysRole.list(this.name || null, null, 1, 50, null).subscribe({
      next: (page) => {
        this.roles.set(page.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  add(): void {
    this.dialog
      .open(SysRoleAddComponent, {
        width: '520px',
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  edit(role: SysRoleItemDto): void {
    this.dialog
      .open(SysRoleEditComponent, {
        width: '520px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: role.id },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  remove(role: SysRoleItemDto): void {
    if (role.isSystem) return;
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.common.confirmDelete),
          content: this.translate.instant(this.i18nKeys.systemRole.deleteConfirm, {
            name: role.name,
          }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.sysRole.delete(role.id).subscribe(() => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.systemRole.deleteSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.load();
        });
      });
  }
}
