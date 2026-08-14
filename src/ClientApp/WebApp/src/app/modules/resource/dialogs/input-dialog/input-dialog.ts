import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { ResourceColorPickerComponent } from 'src/app/modules/resource/shared/color-picker/color-picker';
import { ResourceIconPickerComponent } from 'src/app/modules/resource/shared/icon-picker/icon-picker';

export type ResourceInputDialogFieldType = 'text' | 'textarea' | 'color' | 'icon' | 'select';

export interface ResourceInputDialogOption {
  value: string;
  label: string;
  icon?: string;
}

export interface ResourceInputDialogField {
  key: string;
  label: string;
  value?: string;
  required?: boolean;
  type?: ResourceInputDialogFieldType;
  options?: ResourceInputDialogOption[];
}

export interface ResourceInputDialogData {
  title: string;
  fields: ResourceInputDialogField[];
}

@Component({
  selector: 'app-resource-input-dialog',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSelectModule,
    ResourceColorPickerComponent,
    ResourceIconPickerComponent,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './input-dialog.html',
  styleUrl: './input-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceInputDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly data = inject<ResourceInputDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ResourceInputDialogComponent>);
  readonly form = new FormGroup<Record<string, FormControl<string>>>({});

  constructor() {
    for (const field of this.data.fields) {
      this.form.addControl(
        field.key,
        new FormControl(field.value ?? '', {
          nonNullable: true,
          validators: field.required ? Validators.required : [],
        }),
      );
    }
  }

  save(): void {
    if (this.form.invalid) return;
    this.dialogRef.close(this.form.getRawValue());
  }
}
