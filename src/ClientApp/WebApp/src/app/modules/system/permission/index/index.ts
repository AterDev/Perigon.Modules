import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemPermissionItemDto } from 'src/app/services/admin/models/system-mod/system-permission-item-dto.model';
import { SystemPermissionAddComponent } from 'src/app/modules/system/permission/add/add';
import { SystemPermissionEditComponent } from 'src/app/modules/system/permission/edit/edit';

@Component({
  selector: 'app-system-permission-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemPermissionIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
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

  add(): void {
    this.dialog
      .open(SystemPermissionAddComponent, {
        width: '620px',
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  edit(permission: SystemPermissionItemDto): void {
    this.dialog
      .open(SystemPermissionEditComponent, {
        width: '620px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: permission.id },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  remove(item: SystemPermissionItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.common.confirmDelete),
          content: this.translate.instant(
            this.i18nKeys.systemPermission.deleteConfirm,
            { name: item.name },
          ),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.systemPermission.delete(item.id).subscribe(() => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.systemPermission.deleteSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.load();
        });
      });
  }
}
