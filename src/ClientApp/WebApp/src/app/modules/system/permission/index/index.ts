import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SysDataScopeItemDto } from 'src/app/services/admin/models/system-mod/sys-data-scope-item-dto.model';
import { SysDataScopeGroupItemDto } from 'src/app/services/admin/models/system-mod/sys-data-scope-group-item-dto.model';
import { DataScopeType } from 'src/app/services/admin/models/entity/data-scope-type.model';
import { SysDataScopeAddComponent } from 'src/app/modules/system/permission/add/add';
import { SysDataScopeEditComponent } from 'src/app/modules/system/permission/edit/edit';

@Component({
  selector: 'app-sys-data-scope-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysDataScopeIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
  readonly dataScopes = signal<SysDataScopeItemDto[]>([]);
  readonly groups = signal<SysDataScopeGroupItemDto[]>([]);
  readonly loading = signal(false);
  name = '';

  constructor() {
    this.client.sysDataScopeGroup
      .filter({ pageIndex: 1, pageSize: 100 })
      .subscribe((page) => this.groups.set(page.data));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.sysDataScope
      .filter({ name: this.name || null, pageIndex: 1, pageSize: 100 })
      .subscribe({
        next: (page) => {
          this.dataScopes.set(page.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  groupName(groupId: string): string {
    return this.groups().find((group) => group.id === groupId)?.name ?? groupId;
  }

  scopeTypeKey(scopeType: DataScopeType): string {
    switch (scopeType) {
      case DataScopeType.None:
        return this.i18nKeys.sysDataScope.types.none;
      case DataScopeType.All:
        return this.i18nKeys.sysDataScope.types.all;
      case DataScopeType.Include:
        return this.i18nKeys.sysDataScope.types.include;
    }
    return this.i18nKeys.sysDataScope.types.none;
  }

  add(): void {
    this.dialog
      .open(SysDataScopeAddComponent, {
        width: '620px',
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  edit(permission: SysDataScopeItemDto): void {
    this.dialog
      .open(SysDataScopeEditComponent, {
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

  remove(item: SysDataScopeItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.common.confirmDelete),
          content: this.translate.instant(
            this.i18nKeys.sysDataScope.deleteConfirm,
            { name: item.name },
          ),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.sysDataScope.delete(item.id).subscribe(() => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.sysDataScope.deleteSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.load();
        });
      });
  }
}
