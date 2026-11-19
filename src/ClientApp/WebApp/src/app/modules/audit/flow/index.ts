import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AuditFlowItemDto } from 'src/app/services/admin/models/audit-mod/audit-flow.model';
import { AuditService } from 'src/app/services/admin/services/audit.service';
import { AuditFlowDialogComponent } from './dialog';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-audit-flow-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditFlowIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly flows = signal<AuditFlowItemDto[]>([]);
  readonly loading = signal(false);
  readonly total = signal(0);
  readonly loadError = signal(false);
  readonly statusLabel = (enabled: boolean) => (enabled ? '启用' : '停用');
  private readonly audit = inject(AuditService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  keyword = '';
  sceneCode = '';
  pageIndex = 0;
  pageSize = 20;

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.audit
      .flows(
        this.sceneCode.trim() || null,
        this.keyword.trim() || null,
        null,
        this.pageIndex + 1,
        this.pageSize,
      )
      .subscribe({
        next: (page) => {
          this.flows.set(page.data);
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

  edit(flow?: AuditFlowItemDto): void {
    this.dialog
      .open(AuditFlowDialogComponent, {
        width: '720px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: flow?.id },
      })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.load();
      });
  }

  toggle(flow: AuditFlowItemDto): void {
    this.audit.setFlowEnabled(flow.id, !flow.isEnabled).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.audit.messages.statusUpdated),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2200 },
        );
        this.load();
      },
      error: () =>
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.audit.messages.updateFail),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 3000 },
        ),
    });
  }

  remove(flow: AuditFlowItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.audit.delete),
          content: this.translate.instant(this.i18nKeys.audit.deleteConfirm, {
            name: flow.name,
          }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.audit.deleteFlow(flow.id).subscribe({
          next: () => {
            this.snackBar.open(
              this.translate.instant(this.i18nKeys.audit.messages.deleted),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 2200 },
            );
            this.load();
          },
          error: () =>
            this.snackBar.open(
              this.translate.instant(this.i18nKeys.audit.messages.deleteFail),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 3500 },
            ),
        });
      });
  }
}
