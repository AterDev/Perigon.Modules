import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from '../../../../share/shared-modules';
import { I18N_KEYS } from '../../../../share/i18n-keys';
import { ResValueType } from '../../../../../services/admin/models/entity/res-value-type.model';

@Component({
  selector: 'app-resource-property-dialog',
  imports: CommonFormModules,
  templateUrl: './property-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourcePropertyDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly valueTypes = [
    { label: 'String', value: ResValueType.String },
    { label: 'Number', value: ResValueType.Number },
    { label: 'Boolean', value: ResValueType.Boolean },
    { label: 'Date', value: ResValueType.Date },
    { label: 'URI', value: ResValueType.Uri },
    { label: 'IP address', value: ResValueType.IPAddress },
  ];
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ResourcePropertyDialogComponent>);
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    valueType: [ResValueType.String],
    isRequired: [false],
    maxLength: [200, [Validators.required, Validators.min(1)]],
  });

  save(): void {
    if (this.form.invalid) return;
    this.dialogRef.close({
      ...this.form.getRawValue(),
      sort: 0,
    });
  }
}
