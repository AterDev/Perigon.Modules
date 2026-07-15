import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from '../../../../share/shared-modules';
import { I18N_KEYS } from '../../../../share/i18n-keys';

@Component({
  selector: 'app-resource-group-dialog',
  imports: CommonFormModules,
  templateUrl: './group-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceGroupDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ResourceGroupDialogComponent>);
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
  });

  save(): void {
    if (this.form.invalid) return;
    this.dialogRef.close(this.form.controls.name.value);
  }
}
