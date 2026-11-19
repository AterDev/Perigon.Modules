import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AuditMode } from 'src/app/services/admin/models/audit-mod/audit-enums.model';
import {
  AuditFlowDetailDto,
  AuditFlowStepDto,
  AuditFlowWriteDto,
} from 'src/app/services/admin/models/audit-mod/audit-flow.model';
import { AuditService } from 'src/app/services/admin/services/audit.service';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { UserPickerComponent } from 'src/app/modules/share/components/user-picker/user-picker';
import { TranslateService } from '@ngx-translate/core';

type AuditStepForm = FormGroup<{
  name: FormControl<string>;
  auditMode: FormControl<AuditMode>;
  auditorUserIds: FormControl<string[]>;
}>;

@Component({
  selector: 'app-audit-flow-dialog',
  imports: [...CommonFormModules, UserPickerComponent],
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditFlowDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly detail = signal<AuditFlowDetailDto | null>(null);
  readonly loading = signal(false);
  readonly editing = Boolean(inject<{ id?: string }>(MAT_DIALOG_DATA).id);
  readonly modes = [
    { value: AuditMode.Any, label: '任一人通过' },
    { value: AuditMode.All, label: '全部人通过' },
  ];
  readonly steps = new FormArray<AuditStepForm>([]);
  readonly form = inject(FormBuilder).nonNullable.group({
    flowKey: ['', [Validators.required, Validators.maxLength(100)]],
    sceneCode: ['', [Validators.required, Validators.maxLength(100)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    isEnabled: [false],
  });
  saving = false;
  private readonly data = inject<{ id?: string }>(MAT_DIALOG_DATA);
  private readonly audit = inject(AuditService);
  private readonly dialogRef = inject(MatDialogRef<AuditFlowDialogComponent>);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  constructor() {
    if (!this.data.id) {
      this.addStep();
    }
    if (this.data.id) {
      this.loading.set(true);
      this.audit.flow(this.data.id).subscribe({
        next: (flow) => {
          this.detail.set(flow);
          this.form.patchValue({
            flowKey: flow.flowKey,
            sceneCode: flow.sceneCode,
            name: flow.name,
            isEnabled: flow.isEnabled,
          });
          this.steps.clear();
          flow.steps.forEach((step) => this.steps.push(this.createStep(step)));
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.audit.messages.loadFlowFail),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 3000 },
          );
        },
      });
    }
  }

  addStep(): void {
    this.steps.push(this.createStep());
  }

  removeStep(index: number): void {
    if (this.steps.length > 1) this.steps.removeAt(index);
  }

  save(): void {
    if (
      this.form.invalid ||
      this.loading() ||
      this.steps.length === 0 ||
      this.steps.invalid
    ) {
      this.form.markAllAsTouched();
      this.steps.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const steps: AuditFlowStepDto[] = this.steps.controls.map((step, index) => {
      const current = step.getRawValue();
      return {
        stepNo: index + 1,
        name: current.name.trim(),
        auditMode: current.auditMode,
        auditorUserIds: current.auditorUserIds,
      };
    });
    if (steps.some((step) => !step.name || step.auditorUserIds.length === 0)) {
      this.snackBar.open(
        this.translate.instant(this.i18nKeys.audit.messages.invalidSteps),
        this.translate.instant(this.i18nKeys.common.close),
        {
          duration: 3000,
        },
      );
      return;
    }
    const dto: AuditFlowWriteDto = {
      flowKey: value.flowKey.trim(),
      sceneCode: value.sceneCode.trim(),
      name: value.name.trim(),
      isEnabled: value.isEnabled,
      steps,
    };
    this.saving = true;
    const request = this.data.id
      ? this.audit.updateFlow(this.data.id, dto)
      : this.audit.addFlow(dto);
    request.subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.audit.messages.saved),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2200 },
        );
        this.dialogRef.close(true);
      },
      error: () => {
        this.saving = false;
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.audit.messages.saveFail),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 3500 },
        );
      },
    });
  }

  private createStep(step?: AuditFlowStepDto): AuditStepForm {
    return new FormGroup({
      name: new FormControl(step?.name ?? '', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(200)],
      }),
      auditMode: new FormControl(step?.auditMode ?? AuditMode.Any, {
        nonNullable: true,
      }),
      auditorUserIds: new FormControl(step?.auditorUserIds ?? [], {
        nonNullable: true,
        validators: [Validators.required],
      }),
    });
  }
}
