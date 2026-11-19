import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AuditRecordItemDto } from 'src/app/services/admin/models/audit-mod/audit-record.model';
import { AuditRecordStatus } from 'src/app/services/admin/models/audit-mod/audit-enums.model';
import { AuditService } from 'src/app/services/admin/services/audit.service';
import { AuditRecordDialogComponent } from './dialog';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';

@Component({
  selector: 'app-audit-record-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditRecordIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly records = signal<AuditRecordItemDto[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal(false);
  readonly total = signal(0);
  readonly mine = Boolean(inject(ActivatedRoute).snapshot.data['mine']);
  readonly statuses = [
    { value: AuditRecordStatus.InProgress, label: '审核中' },
    { value: AuditRecordStatus.Approved, label: '已通过' },
    { value: AuditRecordStatus.Rejected, label: '已驳回' },
    { value: AuditRecordStatus.Withdrawn, label: '已撤回' },
  ];
  private readonly audit = inject(AuditService);
  private readonly dialog = inject(MatDialog);
  status: AuditRecordStatus | null = null;
  sceneCode = '';
  bizId = '';
  pageIndex = 0;
  pageSize = 20;

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    const request = this.mine
      ? this.audit.myRecords(
          this.status,
          this.sceneCode,
          this.bizId,
          this.pageIndex + 1,
          this.pageSize,
        )
      : this.audit.records(
          this.status,
          this.sceneCode,
          this.bizId,
          this.pageIndex + 1,
          this.pageSize,
        );
    request.subscribe({
      next: (page) => {
        this.records.set(page.data);
        this.total.set(page.count);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
      },
    });
  }

  pageChanged(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  detail(item: AuditRecordItemDto): void {
    this.dialog
      .open(AuditRecordDialogComponent, {
        width: '820px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: item.id, mine: this.mine },
      })
      .afterClosed()
      .subscribe((changed) => {
        if (changed) this.load();
      });
  }
  statusLabel(status: AuditRecordStatus): string {
    return this.statussLabel(status);
  }
  private statussLabel(status: AuditRecordStatus): string {
    return this.statuses.find((item) => item.value === status)?.label ?? '未知';
  }
}
