import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AuditRecordDetailDto } from 'src/app/services/admin/models/audit-mod/audit-record.model';
import {
  AuditMode,
  AuditRecordStatus,
  AuditStepStatus,
  AuditTaskStatus,
} from 'src/app/services/admin/models/audit-mod/audit-enums.model';
import { AuditService } from 'src/app/services/admin/services/audit.service';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-audit-record-dialog',
  imports: CommonFormModules,
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditRecordDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly detail = signal<AuditRecordDetailDto | null>(null);
  readonly loading = signal(true);
  readonly mode = AuditMode;
  readonly stepStatus = AuditStepStatus;
  readonly taskStatus = AuditTaskStatus;
  readonly mine = Boolean(inject<{ mine?: boolean }>(MAT_DIALOG_DATA).mine);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  private readonly audit = inject(AuditService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
  readonly dialogRef = inject(MatDialogRef<AuditRecordDialogComponent>);
  saving = false;

  constructor() {
    this.audit.record(this.data.id).subscribe({
      next: (item) => {
        this.detail.set(item);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.common.loadFail),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 3000 },
        );
      },
    });
  }
  statusLabel(status: AuditRecordStatus): string {
    return ['审核中', '已通过', '已驳回', '已撤回'][status] ?? '未知';
  }
  stepLabel(status: AuditStepStatus): string {
    return ['等待', '处理中', '已通过', '已驳回', '已取消'][status] ?? '未知';
  }
  taskLabel(status: AuditTaskStatus): string {
    return ['等待', '待处理', '已通过', '已驳回', '已取消'][status] ?? '未知';
  }
  withdraw(): void {
    const item = this.detail();
    if (!item || !this.mine || item.status !== AuditRecordStatus.InProgress)
      return;
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.audit.withdraw),
          content: this.translate.instant(this.i18nKeys.audit.withdrawConfirm),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.saving = true;
        this.audit.withdrawRecord(item.id).subscribe({
          next: () => {
            this.snackBar.open(
              this.translate.instant(this.i18nKeys.audit.messages.withdrawn),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 2500 },
            );
            this.dialogRef.close(true);
          },
          error: () => {
            this.saving = false;
            this.snackBar.open(
              this.translate.instant(this.i18nKeys.audit.messages.withdrawFail),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 3500 },
            );
          },
        });
      });
  }
}
