import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { PersonalResourceDetailDto } from 'src/app/services/admin/models/resource-mod/personal-resource-detail-dto.model';
import { PersonalResourceStatus } from 'src/app/services/admin/models/resource-mod/personal-resource-status.model';
import { PersonalResourceAuditStatus } from 'src/app/services/admin/models/resource-mod/personal-resource-audit-status.model';

@Component({
  selector: 'app-personal-resource-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonalResourceDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly resource = signal<PersonalResourceDetailDto | null>(null);
  private readonly client = inject(AdminClient);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);

  constructor() {
    this.client.personalResource.detail(this.data.id).subscribe((item) => this.resource.set(item));
  }

  statusLabel(status: PersonalResourceStatus): string {
    return status === PersonalResourceStatus.Private
      ? this.i18nKeys.resource.personalPrivate
      : this.i18nKeys.resource.personalApplyPublic;
  }

  auditLabel(status: PersonalResourceAuditStatus): string {
    return [
      this.i18nKeys.resource.auditNotRequired,
      this.i18nKeys.resource.auditPending,
      this.i18nKeys.resource.auditApproved,
      this.i18nKeys.resource.auditRejected,
    ][status];
  }
}
