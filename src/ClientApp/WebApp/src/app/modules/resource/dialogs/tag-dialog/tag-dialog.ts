import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CommonFormModules } from '../../../share/shared-modules';
import { I18N_KEYS } from '../../../share/i18n-keys';
import { ResTag } from '../../../../services/admin/models/entity/res-tag.model';
import { ResTagAddDto } from '../../../../services/admin/models/resource-mod/res-tag-add-dto.model';
import { ResourceColorPickerComponent } from '../../shared/color-picker/color-picker';
import { ResourceIconPickerComponent } from '../../shared/icon-picker/icon-picker';

export interface ResourceTagDialogData {
  tag?: ResTag;
}

@Component({
  selector: 'app-resource-tag-dialog',
  imports: [
    ...CommonFormModules,
    MatDialogModule,
    ResourceColorPickerComponent,
    ResourceIconPickerComponent,
  ],
  templateUrl: './tag-dialog.html',
  styleUrl: './tag-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceTagDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly data = inject<ResourceTagDialogData>(MAT_DIALOG_DATA, { optional: true });
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ResourceTagDialogComponent>);
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    color: ['#ff9800', Validators.required],
    icon: ['label'],
  });

  constructor() {
    const tag = this.data?.tag;
    this.form.patchValue({
      name: tag?.name ?? '',
      color: tag?.color ?? '#ff9800',
      icon: tag?.icon ?? 'label',
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const result: ResTagAddDto = {
      name: value.name.trim(),
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
