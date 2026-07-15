import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from '../../../share/i18n-keys';

export interface ResourceInputDialogField {
  key: string;
  label: string;
  value?: string;
  required?: boolean;
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
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './input-dialog.html',
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
