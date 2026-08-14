import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormControl,
  FormRecord,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ResEnvironment } from 'src/app/services/admin/models/entity/res-environment.model';
import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';
import { ResGroup } from 'src/app/services/admin/models/entity/res-group.model';
import { ResTag } from 'src/app/services/admin/models/entity/res-tag.model';
import { ResDefinition } from 'src/app/services/admin/models/entity/res-definition.model';
import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ResourceGroupDialogComponent } from 'src/app/modules/resource/dialogs/group-dialog/group-dialog';
import { ResourceTagDialogComponent } from 'src/app/modules/resource/dialogs/tag-dialog/tag-dialog';
import { ResGroupAddDto } from 'src/app/services/admin/models/resource-mod/res-group-add-dto.model';
import { ResTagAddDto } from 'src/app/services/admin/models/resource-mod/res-tag-add-dto.model';
import { resourceIconName, resourceIconStyle } from 'src/app/modules/resource/shared/resource-appearance';
import { resourceValueValidator } from 'src/app/modules/resource/shared/resource-value-validation';

@Component({
  selector: 'app-resource-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly iconName = resourceIconName;
  readonly iconStyle = resourceIconStyle;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute, { optional: true });
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);
  private readonly dialogRef = inject(MatDialogRef<ResourceEditComponent>, { optional: true });
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA, { optional: true });
  readonly id = this.data?.id ?? this.route?.snapshot.paramMap.get('id') ?? '';
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly groups = signal<ResGroup[]>([]);
  readonly tags = signal<ResTag[]>([]);
  readonly definitions = signal<ResDefinition[]>([]);
  readonly valueTypes = ResValueType;
  readonly valueTypeLabels = {
    [ResValueType.String]: I18N_KEYS.resource.propertyTypes.string,
    [ResValueType.Number]: I18N_KEYS.resource.propertyTypes.number,
    [ResValueType.Boolean]: I18N_KEYS.resource.propertyTypes.boolean,
    [ResValueType.Date]: I18N_KEYS.resource.propertyTypes.date,
    [ResValueType.Uri]: I18N_KEYS.resource.propertyTypes.uri,
    [ResValueType.IPAddress]: I18N_KEYS.resource.propertyTypes.ipAddress,
  };
  readonly form = this.fb.nonNullable.group({
    environmentId: ['', Validators.required],
    categoryId: ['', Validators.required],
    groupId: [''],
    definitionId: ['', Validators.required],
    tagNames: [[] as string[]],
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
  saving = false;
  constructor() {
    forkJoin({
      environments: this.client.resourceConfiguration.environments(),
      categories: this.client.resourceConfiguration.categories(),
      tags: this.client.resourceConfiguration.tags(),
      definitions: this.client.resourceConfiguration.definitions(null),
      detail: this.client.resource.detail(this.id),
    }).subscribe((result) => {
      this.environments.set(result.environments);
      this.categories.set(result.categories);
      this.tags.set(result.tags);
      this.definitions.set(result.definitions);
      this.form.patchValue({
        environmentId: result.detail.environmentId,
        categoryId: result.detail.categoryId,
        definitionId: result.detail.definitionId,
        tagNames: result.detail.tagNames,
        groupId: result.detail.groupId ?? '',
      });
      this.client.resourceConfiguration
        .groups(result.detail.categoryId)
        .subscribe((value) => this.groups.set(value));
      this.definitionChanged();
      for (const value of result.detail.values)
        this.values.controls[value.definitionPropertyId]?.setValue(value.value);
    });
  }
  categoryChanged(): void {
    this.form.controls.groupId.setValue('');
    const id = this.form.controls.categoryId.value;
    if (id)
      this.client.resourceConfiguration
        .groups(id)
        .subscribe((value) => this.groups.set(value));
  }
  definitionChanged(): void {
    Object.keys(this.values.controls).forEach((key) =>
      this.values.removeControl(key),
    );
    for (const property of this.definition()?.properties ?? [])
      this.values.addControl(
        property.id,
        new FormControl('', {
          nonNullable: true,
          validators: [
            property.isRequired
              ? Validators.required
              : Validators.nullValidator,
            resourceValueValidator(property.valueType),
            Validators.maxLength(property.maxLength),
          ],
        }),
      );
  }
  createTag(): void {
    this.dialog
      .open(ResourceTagDialogComponent, {
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((value: ResTagAddDto | undefined) => {
        if (!value) return;
        this.client.resourceConfiguration
          .addTag(value)
          .subscribe((tag) => {
            this.tags.update((items) => [...items, tag]);
            this.form.controls.tagNames.setValue([
              ...this.form.controls.tagNames.value,
              tag.name,
            ]);
          });
      });
  }
  createGroup(): void {
    const categoryId = this.form.controls.categoryId.value;
    if (!categoryId) return;
    this.dialog
      .open(ResourceGroupDialogComponent, {
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { categoryId },
      })
      .afterClosed()
      .subscribe((value: ResGroupAddDto | undefined) => {
        if (!value) return;
        this.client.resourceConfiguration
          .addGroup(value)
          .subscribe((group) => {
            this.groups.update((items) => [...items, group]);
            this.form.controls.groupId.setValue(group.id);
          });
      });
  }
  save(): void {
    if (this.form.invalid || this.values.invalid) {
      this.form.markAllAsTouched();
      this.values.markAllAsTouched();
      return;
    }
    const base = this.form.getRawValue();
    this.saving = true;
    this.client.resource
      .update(this.id, {
        ...base,
        groupId: base.groupId || null,
        values: Object.entries(this.values.getRawValue())
          .filter(([, value]) => value.length > 0)
          .map(([definitionPropertyId, value]) => ({ definitionPropertyId, value })),
      })
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('resource.updateSuccess'),
            this.translate.instant('common.close'),
            { duration: 2500 },
          );
          if (this.dialogRef) {
            this.dialogRef.close({ saved: true });
          } else {
            this.router.navigate(['/resource', this.id, 'detail']);
          }
        },
        error: () => (this.saving = false),
      });
  }
}
