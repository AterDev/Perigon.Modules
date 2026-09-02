import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { UserResourceDetailDto } from 'src/app/services/admin/models/resource-mod/user-resource-detail-dto.model';
import { UserResourceStatus } from 'src/app/services/admin/models/entity/user-resource-status.model';
import { UserResourceAuditStatus } from 'src/app/services/admin/models/entity/user-resource-audit-status.model';

@Component({
  selector: 'app-user-resource-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserResourceDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly resource = signal<UserResourceDetailDto | null>(null);
  readonly loadError = signal(false);
  private readonly client = inject(AdminClient);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);

  constructor() {
    this.client.userResource.detail(this.data.id).subscribe({
      next: (item) => this.resource.set(item),
      error: () => this.loadError.set(true),
    });
  }

  statusLabel(status: UserResourceStatus): string {
    return status === UserResourceStatus.Private
      ? this.i18nKeys.resource.userPrivate
      : this.i18nKeys.resource.userApplyPublic;
  }

  auditLabel(status: UserResourceAuditStatus): string {
    return [
      this.i18nKeys.resource.auditNotRequired,
      this.i18nKeys.resource.auditPending,
      this.i18nKeys.resource.auditApproved,
      this.i18nKeys.resource.auditRejected,
    ][status] ?? this.i18nKeys.resource.auditPending;
  }
}
