import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { PersonalResourceAuditStatus } from 'src/app/services/admin/models/resource-mod/personal-resource-audit-status.model';
import { PersonalResourceItemDto } from 'src/app/services/admin/models/resource-mod/personal-resource-item-dto.model';
import { PersonalResourceStatus } from 'src/app/services/admin/models/resource-mod/personal-resource-status.model';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { PersonalResourceAddComponent } from 'src/app/modules/resource/personal-resource/add/add';
import { PersonalResourceDetailComponent } from 'src/app/modules/resource/personal-resource/detail/detail';

@Component({
  selector: 'app-personal-resource-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonalResourceIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly statuses = PersonalResourceStatus;
  readonly auditStatuses = PersonalResourceAuditStatus;
  readonly resources = signal<PersonalResourceItemDto[]>([]);
  readonly loading = signal(false);
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.personalResource.mine({ pageIndex: 1, pageSize: 100 }).subscribe({
      next: (page) => {
        this.resources.set(page.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  add(): void {
    this.dialog
      .open(PersonalResourceAddComponent, {
        width: '800px',
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  detail(resource: PersonalResourceItemDto): void {
    this.dialog.open(PersonalResourceDetailComponent, {
      width: '760px',
      maxWidth: '96vw',
      maxHeight: '96vh',
      data: { id: resource.id },
    });
  }

  edit(resource: PersonalResourceItemDto): void {
    this.dialog
      .open(PersonalResourceAddComponent, {
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

  remove(resource: PersonalResourceItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant('common.confirmDelete'),
          content: this.translate.instant('resource.personalDeleteConfirm', {
            name: resource.definitionName,
          }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.personalResource.delete(resource.id).subscribe(() => {
          this.snackBar.open(
            this.translate.instant('resource.personalDeleteSuccess'),
            this.translate.instant('common.close'),
            { duration: 2500 },
          );
          this.load();
        });
      });
  }

  statusLabel(status: PersonalResourceStatus): string {
    return status === PersonalResourceStatus.Private
      ? this.i18nKeys.resource.personalPrivate
      : this.i18nKeys.resource.personalApplyPublic;
  }

  auditLabel(status: PersonalResourceAuditStatus): string {
    const keys = {
      [PersonalResourceAuditStatus.NotRequired]: this.i18nKeys.resource.auditNotRequired,
      [PersonalResourceAuditStatus.Pending]: this.i18nKeys.resource.auditPending,
      [PersonalResourceAuditStatus.Approved]: this.i18nKeys.resource.auditApproved,
      [PersonalResourceAuditStatus.Rejected]: this.i18nKeys.resource.auditRejected,
    };
    return keys[status];
  }
}
