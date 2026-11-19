import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AuditTaskItemDto } from 'src/app/services/admin/models/audit-mod/audit-task.model';
import { AuditTaskStatus } from 'src/app/services/admin/models/audit-mod/audit-enums.model';
import { AuditService } from 'src/app/services/admin/services/audit.service';
import { AuditTaskDialogComponent } from './dialog';

@Component({
  selector: 'app-audit-task-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditTaskIndexComponent {
  readonly tasks = signal<AuditTaskItemDto[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal(false);
  readonly total = signal(0);
  readonly statuses = [
    { value: AuditTaskStatus.Pending, label: '待处理' },
    { value: AuditTaskStatus.Approved, label: '已通过' },
    { value: AuditTaskStatus.Rejected, label: '已驳回' },
    { value: AuditTaskStatus.Cancelled, label: '已取消' },
  ];
  private readonly audit = inject(AuditService);
  private readonly dialog = inject(MatDialog);
  status: AuditTaskStatus | null = AuditTaskStatus.Pending;
  pageIndex = 0;
  pageSize = 20;

  constructor() {
    this.load();
  }
  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.audit.tasks(this.status, this.pageIndex + 1, this.pageSize).subscribe({
      next: (page) => {
        this.tasks.set(page.data);
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
  open(task: AuditTaskItemDto): void {
    this.dialog
      .open(AuditTaskDialogComponent, {
        width: '820px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: task.id },
      })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.load();
      });
  }
  statusLabel(status: AuditTaskStatus): string {
    return ['等待', '待处理', '已通过', '已驳回', '已取消'][status] ?? '未知';
  }
}
