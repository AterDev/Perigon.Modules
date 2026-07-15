import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from '../../../../share/shared-modules';
import { I18N_KEYS } from '../../../../share/i18n-keys';
import { ResDefinition } from '../../../../../services/admin/models/entity/res-definition.model';
import { ResValueType } from '../../../../../services/admin/models/entity/res-value-type.model';
import { ResDefinitionInput } from '../../../../../services/admin/models/resource-mod/res-definition-input.model';

export interface ResourceDefinitionDialogData {
  definition?: ResDefinition;
}

type PropertyForm = FormGroup<{
  id: FormControl<string | null>;
  name: FormControl<string>;
  valueType: FormControl<ResValueType>;
  isRequired: FormControl<boolean>;
  maxLength: FormControl<number>;
}>;

@Component({
  selector: 'app-resource-definition-dialog',
  imports: CommonFormModules,
  templateUrl: './definition-dialog.html',
  styleUrl: './definition-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceDefinitionDialogComponent {
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
  private readonly dialogRef = inject(MatDialogRef<ResourceDefinitionDialogComponent>);
  readonly data = inject<ResourceDefinitionDialogData>(MAT_DIALOG_DATA, { optional: true });
  readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control('', Validators.required),
    properties: this.formBuilder.array<PropertyForm>([]),
  });

  constructor() {
    this.form.controls.name.setValue(this.data?.definition?.name ?? '');
    for (const property of this.data?.definition?.properties ?? []) {
      this.addProperty(property);
    }
  }

  get properties(): FormArray<PropertyForm> {
    return this.form.controls.properties;
  }

  addProperty(property?: ResDefinition['properties'][number]): void {
    this.properties.push(this.createPropertyForm(property));
  }

  removeProperty(index: number): void {
    this.properties.removeAt(index);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const result: ResDefinitionInput = {
      name: value.name.trim(),
      icon: this.data?.definition?.icon ?? 'schema',
      properties: value.properties.map((property, index) => ({
        id: property.id,
        name: property.name.trim(),
        valueType: property.valueType,
        isRequired: property.isRequired,
        maxLength: property.maxLength,
        sort: index,
      })),
    };

    if (!result.name) {
      this.form.controls.name.setErrors({ required: true });
      return;
    }

    this.dialogRef.close(result);
  }

  private createPropertyForm(property?: ResDefinition['properties'][number]): PropertyForm {
    return this.formBuilder.group({
      id: this.formBuilder.control<string | null>(property?.id ?? null),
      name: this.formBuilder.nonNullable.control(property?.name ?? '', Validators.required),
      valueType: this.formBuilder.nonNullable.control(property?.valueType ?? ResValueType.String),
      isRequired: this.formBuilder.nonNullable.control(property?.isRequired ?? false),
      maxLength: this.formBuilder.nonNullable.control(property?.maxLength ?? 200, [
        Validators.required,
        Validators.min(1),
        Validators.max(1000),
      ]),
    });
  }
}
