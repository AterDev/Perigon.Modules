import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormRecord,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResEnvironment } from '../../../../services/admin/models/entity/res-environment.model';
import { ResCategory } from '../../../../services/admin/models/entity/res-category.model';
import { ResGroup } from '../../../../services/admin/models/entity/res-group.model';
import { ResTag } from '../../../../services/admin/models/entity/res-tag.model';
import { ResDefinition } from '../../../../services/admin/models/entity/res-definition.model';
import { ResValueType } from '../../../../services/admin/models/entity/res-value-type.model';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-resource-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly groups = signal<ResGroup[]>([]);
  readonly tags = signal<ResTag[]>([]);
  readonly definitions = signal<ResDefinition[]>([]);
  readonly valueTypes = ResValueType;
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    iconUrl: [''],
    description: [''],
    environmentId: ['', Validators.required],
    categoryId: ['', Validators.required],
    groupId: [''],
    definitionId: ['', Validators.required],
    tagNames: [[] as string[]],
  });
  readonly values = new FormRecord<FormControl<string>>({});
  readonly definition = computed(
    () =>
      this.definitions().find(
        (item) => item.id === this.form.controls.definitionId.value,
      ) ?? null,
  );
  saving = false;
  constructor() {
    forkJoin({
      environments: this.client.resourceConfiguration.environments(),
      categories: this.client.resourceConfiguration.categories(),
      tags: this.client.resourceConfiguration.tags(),
      definitions: this.client.resourceConfiguration.definitions(),
      detail: this.client.resource.detail(this.id),
    }).subscribe((result) => {
      this.environments.set(result.environments);
      this.categories.set(result.categories);
      this.tags.set(result.tags);
      this.definitions.set(result.definitions);
      this.form.patchValue({
        ...result.detail,
        iconUrl: result.detail.iconUrl ?? '',
        description: result.detail.description ?? '',
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
            Validators.maxLength(property.maxLength),
          ],
        }),
      );
  }
  createTag(): void {
    const name = prompt(this.translate.instant('resource.newTagNamePrompt'));
    if (!name) return;
    this.client.resourceConfiguration
      .addTag({ name, color: '#607d8b', icon: 'label' })
      .subscribe((tag) => {
        this.tags.update((items) => [...items, tag]);
        this.form.controls.tagNames.setValue([
          ...this.form.controls.tagNames.value,
          tag.name,
        ]);
      });
  }
  createGroup(): void {
    const categoryId = this.form.controls.categoryId.value;
    if (!categoryId) return;
    const name = prompt(this.translate.instant('resource.newGroupNamePrompt'));
    if (!name) return;
    this.client.resourceConfiguration
      .addGroup({
        name,
        categoryId,
        color: '#607d8b',
        icon: 'folder',
        description: null,
      })
      .subscribe((group) => {
        this.groups.update((items) => [...items, group]);
        this.form.controls.groupId.setValue(group.id);
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
        iconUrl: base.iconUrl || null,
        description: base.description || null,
        groupId: base.groupId || null,
        values: Object.entries(this.values.getRawValue()).map(
          ([definitionPropertyId, value]) => ({ definitionPropertyId, value }),
        ),
      })
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('resource.updateSuccess'),
            this.translate.instant('common.close'),
            { duration: 2500 },
          );
          this.router.navigate(['/resource', this.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
