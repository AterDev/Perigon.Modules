import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, FormControl, FormRecord, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { forkJoin } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ResDefinition } from 'src/app/services/admin/models/entity/res-definition.model';
import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';
import { PersonalResourceStatus } from 'src/app/services/admin/models/resource-mod/personal-resource-status.model';
import { resourceValueValidator } from 'src/app/modules/resource/shared/resource-value-validation';

interface PersonalResourceDialogData {
  id?: string;
}

@Component({
  selector: 'app-personal-resource-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonalResourceAddComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly statuses = PersonalResourceStatus;
  readonly valueTypes = ResValueType;
  readonly valueTypeLabels = {
    [ResValueType.String]: I18N_KEYS.resource.propertyTypes.string,
    [ResValueType.Number]: I18N_KEYS.resource.propertyTypes.number,
    [ResValueType.Boolean]: I18N_KEYS.resource.propertyTypes.boolean,
    [ResValueType.Date]: I18N_KEYS.resource.propertyTypes.date,
    [ResValueType.Uri]: I18N_KEYS.resource.propertyTypes.uri,
    [ResValueType.IPAddress]: I18N_KEYS.resource.propertyTypes.ipAddress,
  };
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialogRef = inject(MatDialogRef<PersonalResourceAddComponent>, {
    optional: true,
  });
  private readonly data = inject<PersonalResourceDialogData>(MAT_DIALOG_DATA, {
    optional: true,
  });
  readonly id = this.data?.id ?? null;
  readonly definitions = signal<ResDefinition[]>([]);
  readonly form = this.fb.nonNullable.group({
    definitionId: ['', Validators.required],
    status: [PersonalResourceStatus.Private, Validators.required],
  });
  private readonly selectedDefinitionId = toSignal(
    this.form.controls.definitionId.valueChanges,
    { initialValue: this.form.controls.definitionId.value },
  );
  readonly values = new FormRecord<FormControl<string>>({});
  readonly definition = computed(
    () =>
      this.definitions().find(
        (item) => item.id === this.selectedDefinitionId(),
      ) ?? null,
  );
  readonly title = computed(() =>
    this.id ? this.i18nKeys.resource.personalEditTitle : this.i18nKeys.resource.personalAddTitle,
  );
  saving = false;

  constructor() {
    const definitions = this.client.resourceConfiguration.definitions(null);
    if (this.id) {
      forkJoin({
        definitions,
        detail: this.client.personalResource.detail(this.id),
      }).subscribe((result) => {
        this.definitions.set(result.definitions);
        this.form.patchValue({
          definitionId: result.detail.definitionId,
          status: result.detail.status,
        });
        this.definitionChanged();
        for (const value of result.detail.values) {
          this.values.controls[value.definitionPropertyId]?.setValue(value.value);
        }
      });
    } else {
      definitions.subscribe((items) => {
        this.definitions.set(items);
        if (items.length > 0) {
          this.form.controls.definitionId.setValue(items[0].id);
          this.definitionChanged();
        }
      });
    }
  }

  definitionChanged(): void {
    Object.keys(this.values.controls).forEach((key) => this.values.removeControl(key));
    for (const property of this.definition()?.properties ?? []) {
      this.values.addControl(
        property.id,
        new FormControl('', {
          nonNullable: true,
          validators: [
            property.isRequired ? Validators.required : Validators.nullValidator,
            resourceValueValidator(property.valueType),
            Validators.maxLength(property.maxLength),
          ],
        }),
      );
    }
  }

  save(): void {
    if (this.form.invalid || this.values.invalid) {
      this.form.markAllAsTouched();
      this.values.markAllAsTouched();
      return;
    }

    const data = {
      ...this.form.getRawValue(),
      values: Object.entries(this.values.getRawValue())
        .filter(([, value]) => value.length > 0)
        .map(([definitionPropertyId, value]) => ({ definitionPropertyId, value })),
    };
    this.saving = true;
    const saved = () => {
      this.snackBar.open(
        this.translate.instant(
          this.id ? 'resource.personalUpdateSuccess' : 'resource.personalCreateSuccess',
        ),
        this.translate.instant('common.close'),
        { duration: 2500 },
      );
      this.dialogRef?.close({ saved: true, id: this.id ?? '' });
    };
    if (this.id) {
      this.client.personalResource.update(this.id, data).subscribe({
        next: saved,
        error: () => (this.saving = false),
      });
    } else {
      this.client.personalResource.add(data).subscribe({
        next: saved,
        error: () => (this.saving = false),
      });
    }
  }
}
