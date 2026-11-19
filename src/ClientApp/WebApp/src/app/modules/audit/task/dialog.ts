import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AuditTaskDetailDto } from 'src/app/services/admin/models/audit-mod/audit-task.model';
import { AuditTaskStatus } from 'src/app/services/admin/models/audit-mod/audit-enums.model';
import { AuditService } from 'src/app/services/admin/services/audit.service';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-audit-task-dialog',
  imports: CommonFormModules,
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditTaskDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly detail = signal<AuditTaskDetailDto | null>(null);
  readonly loading = signal(true);
  readonly status = AuditTaskStatus;
  readonly form = inject(FormBuilder).nonNullable.group({ comment: [''] });
  saving = false;
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  private readonly audit = inject(AuditService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialogRef = inject(MatDialogRef<AuditTaskDialogComponent>);

  constructor() {
    this.audit.task(this.data.id).subscribe({
      next: (item) => {
        this.detail.set(item);
        this.form.patchValue({ comment: item.comment ?? '' });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.audit.messages.loadTaskFail),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 3000 },
        );
      },
    });
  }
  decide(approved: boolean): void {
    if (this.loading() || this.saving || !this.detail()) return;
    this.saving = true;
    const request = approved
      ? this.audit.approveTask(this.data.id, {
          comment: this.form.controls.comment.value || null,
        })
      : this.audit.rejectTask(this.data.id, {
          comment: this.form.controls.comment.value || null,
        });
    request.subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(
            approved
              ? this.i18nKeys.audit.messages.approved
              : this.i18nKeys.audit.messages.rejected,
          ),
          this.translate.instant(this.i18nKeys.common.close),
          {
            duration: 2500,
          },
        );
        this.dialogRef.close(true);
      },
      error: () => {
        this.saving = false;
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.audit.messages.taskFail),
          this.translate.instant(this.i18nKeys.common.close),
          {
            duration: 3500,
          },
        );
      },
    });
  }
}
