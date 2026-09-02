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
import { UserResourceStatus } from 'src/app/services/admin/models/entity/user-resource-status.model';
import { UserResourceAddDto } from 'src/app/services/admin/models/resource-mod/user-resource-add-dto.model';
import { UserResourceUpdateDto } from 'src/app/services/admin/models/resource-mod/user-resource-update-dto.model';
import { resourceValueValidator } from 'src/app/modules/resource/shared/resource-value-validation';

interface UserResourceDialogData {
  id?: string;
}

@Component({
  selector: 'app-user-resource-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserResourceAddComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly statuses = UserResourceStatus;
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
  private readonly dialogRef = inject(MatDialogRef<UserResourceAddComponent>, {
    optional: true,
  });
  private readonly data = inject<UserResourceDialogData>(MAT_DIALOG_DATA, {
    optional: true,
  });
  readonly id = this.data?.id ?? null;
  readonly definitions = signal<ResDefinition[]>([]);
  readonly form = this.fb.nonNullable.group({
    definitionId: ['', Validators.required],
    status: [UserResourceStatus.Private, Validators.required],
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
    this.id ? this.i18nKeys.resource.userEditTitle : this.i18nKeys.resource.userAddTitle,
  );
  readonly loading = signal(true);
  readonly loadError = signal(false);
  saving = false;

  constructor() {
    const definitions = this.client.resourceConfiguration.definitions(null);
    if (this.id) {
      forkJoin({
        definitions,
        detail: this.client.userResource.detail(this.id),
      }).subscribe({
        next: (result) => {
          this.setDefinitions(result.definitions);
          this.form.patchValue({
            definitionId: result.detail.definitionId,
            status: result.detail.status,
          });
          this.definitionChanged();
          for (const value of result.detail.values) {
            this.values.controls[value.definitionPropertyId]?.setValue(value.value);
          }
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.loadError.set(true);
        },
      });
    } else {
      definitions.subscribe({
        next: (items) => {
          this.setDefinitions(items);
          this.loading.set(false);
          if (items.length > 0) {
            this.form.controls.definitionId.setValue(items[0].id);
            this.definitionChanged();
          }
        },
        error: () => {
          this.loading.set(false);
          this.loadError.set(true);
        },
      });
    }
  }

  private setDefinitions(items: ResDefinition[]): void {
    this.definitions.set(items);
    if (!this.form.controls.definitionId.value && items.length > 0) {
      this.form.controls.definitionId.setValue(items[0].id);
      this.definitionChanged();
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

    const payload = {
      ...this.form.getRawValue(),
      values: Object.entries(this.values.getRawValue())
        .filter(([, value]) => value.trim().length > 0)
        .map(([definitionPropertyId, value]) => ({ definitionPropertyId, value })),
    };
    this.saving = true;
    const onSuccess = (id: string): void => {
      this.snackBar.open(
        this.translate.instant(
          this.id ? this.i18nKeys.resource.userUpdateSuccess : this.i18nKeys.resource.userCreateSuccess,
        ),
        this.translate.instant(this.i18nKeys.common.close),
        { duration: 2500 },
      );
      this.dialogRef?.close({ saved: true, id });
    };
    const onError = (): void => {
      this.saving = false;
      this.snackBar.open(
        this.translate.instant(this.i18nKeys.common.saveFail),
        this.translate.instant(this.i18nKeys.common.close),
        { duration: 2500 },
      );
    };

    if (this.id) {
      this.client.userResource.update(this.id, payload as UserResourceUpdateDto).subscribe({
        next: () => onSuccess(this.id!),
        error: onError,
      });
    } else {
      this.client.userResource.add(payload as UserResourceAddDto).subscribe({
        next: (result) => onSuccess(result.id),
        error: onError,
      });
    }
  }
}
