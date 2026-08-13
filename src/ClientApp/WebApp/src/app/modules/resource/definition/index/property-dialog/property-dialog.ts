import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from '../../../../share/shared-modules';
import { I18N_KEYS } from '../../../../share/i18n-keys';
import { ResDefinitionProperty } from '../../../../../services/admin/models/entity/res-definition-property.model';
import { ResValueType } from '../../../../../services/admin/models/entity/res-value-type.model';

export interface ResourcePropertyDialogData {
  property?: ResDefinitionProperty;
}

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
  readonly data = inject<ResourcePropertyDialogData>(MAT_DIALOG_DATA, { optional: true });
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    valueType: [ResValueType.String],
    isRequired: [false],
    maxLength: [200, [Validators.required, Validators.min(1), Validators.max(1000)]],
  });

  constructor() {
    const property = this.data?.property;
    if (property) {
      this.form.patchValue({
        name: property.name,
        valueType: property.valueType,
        isRequired: property.isRequired,
        maxLength: property.maxLength,
      });
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    if (!value.name.trim()) {
      this.form.controls.name.setErrors({ required: true });
      return;
    }
    this.dialogRef.close({
      ...value,
      name: value.name.trim(),
    });
  }
}
