import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from '../../../share/shared-modules';
import { I18N_KEYS } from '../../../share/i18n-keys';
import { ResCategory } from '../../../../services/admin/models/entity/res-category.model';
import { ResGroup } from '../../../../services/admin/models/entity/res-group.model';
import { ResGroupAddDto } from '../../../../services/admin/models/resource-mod/res-group-add-dto.model';
import { RESOURCE_DEFAULT_COLOR } from '../../shared/resource-appearance';
import { ResourceColorPickerComponent } from '../../shared/color-picker/color-picker';
import { ResourceIconPickerComponent } from '../../shared/icon-picker/icon-picker';
import { resourceIconName, resourceIconStyle } from '../../shared/resource-appearance';

export interface ResourceGroupDialogData {
  group?: ResGroup;
  categoryId?: string;
  categories?: ResCategory[];
}

@Component({
  selector: 'app-resource-group-dialog',
  imports: [
    ...CommonFormModules,
    MatDialogModule,
    ResourceColorPickerComponent,
    ResourceIconPickerComponent,
  ],
  templateUrl: './group-dialog.html',
  styleUrl: './group-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceGroupDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly iconName = resourceIconName;
  readonly iconStyle = resourceIconStyle;
  readonly data = inject<ResourceGroupDialogData>(MAT_DIALOG_DATA, { optional: true });
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ResourceGroupDialogComponent>);
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    categoryId: ['', Validators.required],
    color: [RESOURCE_DEFAULT_COLOR, Validators.required],
    icon: ['folder'],
  });

  constructor() {
    const group = this.data?.group;
    this.form.patchValue({
      name: group?.name ?? '',
      description: group?.description ?? '',
      categoryId: group?.categoryId ?? this.data?.categoryId ?? '',
      color: group?.color ?? RESOURCE_DEFAULT_COLOR,
      icon: group?.icon ?? 'folder',
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const result: ResGroupAddDto = {
      name: value.name.trim(),
      description: value.description.trim() || null,
      categoryId: value.categoryId,
      color: value.color,
      icon: value.icon || null,
    };
    if (!result.name) {
      this.form.controls.name.setErrors({ required: true });
      return;
    }
    this.dialogRef.close(result);
  }
}
