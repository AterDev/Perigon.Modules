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
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResEnvironment } from '../../../../services/admin/models/entity/res-environment.model';
import { ResCategory } from '../../../../services/admin/models/entity/res-category.model';
import { ResGroup } from '../../../../services/admin/models/entity/res-group.model';
import { ResTag } from '../../../../services/admin/models/entity/res-tag.model';
import { ResDefinition } from '../../../../services/admin/models/entity/res-definition.model';
import { ResValueType } from '../../../../services/admin/models/entity/res-value-type.model';

@Component({
  selector: 'app-resource-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceAddComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
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
    this.client.resourceConfiguration
      .environments()
      .subscribe((value) => this.environments.set(value));
    this.client.resourceConfiguration
      .categories()
      .subscribe((value) => this.categories.set(value));
    this.client.resourceConfiguration
      .tags()
      .subscribe((value) => this.tags.set(value));
    this.client.resourceConfiguration
      .definitions()
      .subscribe((value) => this.definitions.set(value));
  }
  categoryChanged(): void {
    this.form.controls.groupId.setValue('');
    const id = this.form.controls.categoryId.value;
    if (id)
      this.client.resourceConfiguration
        .groups(id)
        .subscribe((value) => this.groups.set(value));
    else this.groups.set([]);
  }
  definitionChanged(): void {
    Object.keys(this.values.controls).forEach((key) =>
      this.values.removeControl(key),
    );
    for (const property of this.definition()?.properties ?? []) {
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
  }
  createTag(): void {
    const name = prompt('新标签名称');
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
    if (!categoryId) {
      this.snackBar.open('请先选择分类', '关闭', { duration: 2500 });
      return;
    }
    const name = prompt('新分组名称');
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
      .add({
        ...base,
        iconUrl: base.iconUrl || null,
        description: base.description || null,
        groupId: base.groupId || null,
        values: Object.entries(this.values.getRawValue()).map(
          ([definitionPropertyId, value]) => ({ definitionPropertyId, value }),
        ),
      })
      .subscribe({
        next: (resource) => {
          this.snackBar.open('资源已创建', '关闭', { duration: 2500 });
          this.router.navigate(['/resource', resource.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
