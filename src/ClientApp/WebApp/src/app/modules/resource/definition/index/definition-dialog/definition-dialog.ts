import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatDialog } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { ResDefinition } from 'src/app/services/admin/models/entity/res-definition.model';
import { ResDefinitionProperty } from 'src/app/services/admin/models/entity/res-definition-property.model';
import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';
import { ResDefinitionAddDto } from 'src/app/services/admin/models/resource-mod/res-definition-add-dto.model';
import { ResourceIconPickerComponent } from 'src/app/modules/resource/shared/icon-picker/icon-picker';
import { ResourcePropertySelectDialogComponent } from 'src/app/modules/resource/definition/index/property-select-dialog/property-select-dialog';

export interface ResourceDefinitionDialogData {
  definition?: ResDefinition;
}

type PropertyForm = FormGroup<{
  id: FormControl<string | null>;
  name: FormControl<string>;
  valueType: FormControl<ResValueType>;
  isRequired: FormControl<boolean>;
  maxLength: FormControl<number>;
  isShared: FormControl<boolean>;
}>;

@Component({
  selector: 'app-resource-definition-dialog',
  imports: [...CommonFormModules, ResourceIconPickerComponent],
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
  private readonly dialog = inject(MatDialog);
  private readonly dialogRef = inject(MatDialogRef<ResourceDefinitionDialogComponent>);
  readonly data = inject<ResourceDefinitionDialogData>(MAT_DIALOG_DATA, { optional: true });
  readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control('', Validators.required),
    icon: this.formBuilder.nonNullable.control('schema'),
    properties: this.formBuilder.array<PropertyForm>([]),
  });

  constructor() {
    this.form.controls.name.setValue(this.data?.definition?.name ?? '');
    this.form.controls.icon.setValue(this.data?.definition?.icon ?? 'schema');
    for (const property of this.data?.definition?.properties ?? []) {
      this.addProperty(property, true);
    }
  }

  get properties(): FormArray<PropertyForm> {
    return this.form.controls.properties;
  }

  addProperty(property?: ResDefinition['properties'][number], shared = false): void {
    this.properties.push(this.createPropertyForm(property, shared || !!property?.id));
  }

  selectProperty(): void {
    const selectedIds = this.properties.controls
      .map((control) => control.controls.id.value)
      .filter((id): id is string => !!id);
    this.dialog
      .open(ResourcePropertySelectDialogComponent, {
        width: '600px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { excludeIds: selectedIds },
      })
      .afterClosed()
      .subscribe((selected: ResDefinitionProperty[] | undefined) => {
        for (const property of selected ?? []) {
          this.addProperty(property, true);
        }
      });
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
    const result: ResDefinitionAddDto = {
      name: value.name.trim(),
      icon: value.icon || null,
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

  private createPropertyForm(
    property?: ResDefinition['properties'][number],
    shared = false,
  ): PropertyForm {
    const form = this.formBuilder.group({
      id: this.formBuilder.control<string | null>(property?.id ?? null),
      name: this.formBuilder.nonNullable.control(property?.name ?? '', Validators.required),
      valueType: this.formBuilder.nonNullable.control(property?.valueType ?? ResValueType.String),
      isRequired: this.formBuilder.nonNullable.control(property?.isRequired ?? false),
      maxLength: this.formBuilder.nonNullable.control(property?.maxLength ?? 200, [
        Validators.required,
        Validators.min(1),
        Validators.max(1000),
      ]),
      isShared: this.formBuilder.nonNullable.control(shared),
    });
    if (shared) {
      form.controls.name.disable();
      form.controls.valueType.disable();
      form.controls.isRequired.disable();
      form.controls.maxLength.disable();
    }
    return form;
  }
}
