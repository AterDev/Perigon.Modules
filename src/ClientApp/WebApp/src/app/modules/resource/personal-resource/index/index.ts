import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { UserResourceAuditStatus } from 'src/app/services/admin/models/entity/user-resource-audit-status.model';
import { UserResourceItemDto } from 'src/app/services/admin/models/resource-mod/user-resource-item-dto.model';
import { UserResourceStatus } from 'src/app/services/admin/models/entity/user-resource-status.model';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { UserResourceAddComponent } from 'src/app/modules/resource/personal-resource/add/add';
import { UserResourceDetailComponent } from 'src/app/modules/resource/personal-resource/detail/detail';

@Component({
  selector: 'app-user-resource-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserResourceIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly statuses = UserResourceStatus;
  readonly auditStatuses = UserResourceAuditStatus;
  readonly resources = signal<UserResourceItemDto[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal(false);
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.client.userResource.mine(null, null, 1, 100, null).subscribe({
      next: (page) => {
        this.resources.set(page.data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
      },
    });
  }

  add(): void {
    this.dialog
      .open(UserResourceAddComponent, {
        width: '800px',
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  detail(resource: UserResourceItemDto): void {
    this.dialog.open(UserResourceDetailComponent, {
      width: '760px',
      maxWidth: '96vw',
      maxHeight: '96vh',
      data: { id: resource.id },
    });
  }

  edit(resource: UserResourceItemDto): void {
    this.dialog
      .open(UserResourceAddComponent, {
        width: '800px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: resource.id },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  remove(resource: UserResourceItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.common.confirmDelete),
          content: this.translate.instant(this.i18nKeys.resource.userDeleteConfirm, {
            name: resource.definitionName,
          }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.userResource.delete(resource.id).subscribe({
          next: () => {
            this.snackBar.open(
              this.translate.instant(this.i18nKeys.resource.userDeleteSuccess),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 2500 },
            );
            this.load();
          },
          error: () =>
            this.snackBar.open(
              this.translate.instant(this.i18nKeys.common.deleteFail),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 2500 },
            ),
        });
      });
  }

  statusLabel(status: UserResourceStatus): string {
    return status === UserResourceStatus.Private
      ? this.i18nKeys.resource.userPrivate
      : this.i18nKeys.resource.userApplyPublic;
  }

  auditLabel(status: UserResourceAuditStatus): string {
    const keys = {
      [UserResourceAuditStatus.NotRequired]: this.i18nKeys.resource.auditNotRequired,
      [UserResourceAuditStatus.Pending]: this.i18nKeys.resource.auditPending,
      [UserResourceAuditStatus.Approved]: this.i18nKeys.resource.auditApproved,
      [UserResourceAuditStatus.Rejected]: this.i18nKeys.resource.auditRejected,
    };
    return keys[status] ?? this.i18nKeys.resource.auditPending;
  }
}
