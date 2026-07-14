import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResEnvironment } from '../../../../services/admin/models/entity/res-environment.model';
import { ResCategory } from '../../../../services/admin/models/entity/res-category.model';
import { ResGroup } from '../../../../services/admin/models/entity/res-group.model';
import { ResTag } from '../../../../services/admin/models/entity/res-tag.model';
import { ResDefinition } from '../../../../services/admin/models/entity/res-definition.model';
import { SystemRole } from '../../../../services/admin/models/entity/system-role.model';
import { ResValueType } from '../../../../services/admin/models/entity/res-value-type.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-resource-config-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceConfigIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly groups = signal<ResGroup[]>([]);
  readonly tags = signal<ResTag[]>([]);
  readonly definitions = signal<ResDefinition[]>([]);
  readonly roles = signal<SystemRole[]>([]);
  groupCategoryId = '';
  permissionEnvironmentId = '';
  permissionCategoryId = '';
  permissionRoleIds: string[] = [];
  constructor() {
    this.load();
  }
  load(): void {
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
    this.client.resourceConfiguration
      .roles()
      .subscribe((value) => this.roles.set(value));
  }
  loadGroups(): void {
    if (!this.groupCategoryId) {
      this.groups.set([]);
      return;
    }
    this.client.resourceConfiguration
      .groups(this.groupCategoryId)
      .subscribe((value) => this.groups.set(value));
  }
  createEnvironment(): void {
    const name = prompt(
      this.translate.instant('resource.environmentNamePrompt'),
    );
    if (!name) return;
    this.client.resourceConfiguration
      .addEnvironment({ name, icon: 'cloud', color: '#3f51b5' })
      .subscribe(() => this.load());
  }
  editEnvironment(item: ResEnvironment): void {
    const name = prompt(
      this.translate.instant('resource.environmentNamePrompt'),
      item.name,
    );
    if (!name) return;
    this.client.resourceConfiguration
      .updateEnvironment(item.id, { name, icon: item.icon, color: item.color })
      .subscribe(() => this.load());
  }
  deleteEnvironment(item: ResEnvironment): void {
    if (
      confirm(
        this.translate.instant('resource.deleteEnvironmentConfirm', {
          name: item.name,
        }),
      )
    )
      this.client.resourceConfiguration
        .deleteEnvironment(item.id)
        .subscribe(() => this.load());
  }
  createCategory(): void {
    const name = prompt(this.translate.instant('resource.categoryNamePrompt'));
    const catalogCode = name
      ? prompt(this.translate.instant('resource.categoryCodePrompt'))
      : null;
    if (!name || !catalogCode) return;
    this.client.resourceConfiguration
      .addCategory({ name, catalogCode, icon: 'category', color: '#009688' })
      .subscribe(() => this.load());
  }
  editCategory(item: ResCategory): void {
    const name = prompt(
      this.translate.instant('resource.categoryNamePrompt'),
      item.name,
    );
    const catalogCode = name
      ? prompt(
          this.translate.instant('resource.categoryCodePrompt'),
          item.catalogCode,
        )
      : null;
    if (!name || !catalogCode) return;
    this.client.resourceConfiguration
      .updateCategory(item.id, {
        name,
        catalogCode,
        icon: item.icon,
        color: item.color,
      })
      .subscribe(() => this.load());
  }
  deleteCategory(item: ResCategory): void {
    if (
      confirm(
        this.translate.instant('resource.deleteCategoryConfirm', {
          name: item.name,
        }),
      )
    )
      this.client.resourceConfiguration
        .deleteCategory(item.id)
        .subscribe(() => this.load());
  }
  createGroup(): void {
    if (!this.groupCategoryId) return;
    const name = prompt(this.translate.instant('resource.groupNamePrompt'));
    if (!name) return;
    this.client.resourceConfiguration
      .addGroup({
        name,
        categoryId: this.groupCategoryId,
        icon: 'folder',
        color: '#607d8b',
        description: null,
      })
      .subscribe(() => this.loadGroups());
  }
  editGroup(item: ResGroup): void {
    const name = prompt(
      this.translate.instant('resource.groupNamePrompt'),
      item.name,
    );
    if (!name) return;
    this.client.resourceConfiguration
      .updateGroup(item.id, {
        name,
        categoryId: item.categoryId,
        icon: item.icon,
        color: item.color,
        description: item.description,
      })
      .subscribe(() => this.loadGroups());
  }
  deleteGroup(item: ResGroup): void {
    if (
      confirm(
        this.translate.instant('resource.deleteGroupConfirm', {
          name: item.name,
        }),
      )
    )
      this.client.resourceConfiguration
        .deleteGroup(item.id)
        .subscribe(() => this.loadGroups());
  }
  createTag(): void {
    const name = prompt(this.translate.instant('resource.tagNamePrompt'));
    if (!name) return;
    this.client.resourceConfiguration
      .addTag({ name, icon: 'label', color: '#ff9800' })
      .subscribe(() => this.load());
  }
  editTag(item: ResTag): void {
    const name = prompt(
      this.translate.instant('resource.tagNamePrompt'),
      item.name,
    );
    if (!name) return;
    this.client.resourceConfiguration
      .updateTag(item.id, { name, icon: item.icon, color: item.color })
      .subscribe(() => this.load());
  }
  deleteTag(item: ResTag): void {
    if (
      confirm(
        this.translate.instant('resource.deleteTagConfirm', {
          name: item.name,
        }),
      )
    )
      this.client.resourceConfiguration
        .deleteTag(item.id)
        .subscribe(() => this.load());
  }
  createDefinition(): void {
    const name = prompt(
      this.translate.instant('resource.definitionNamePrompt'),
    );
    if (!name) return;
    const propertyName = prompt(
      this.translate.instant('resource.firstPropertyNamePrompt'),
    );
    const properties = propertyName
      ? [
          {
            name: propertyName,
            valueType: ResValueType.String,
            isRequired: false,
            maxLength: 200,
            sort: 0,
            id: null,
          },
        ]
      : [];
    this.client.resourceConfiguration
      .addDefinition({ name, icon: 'schema', properties })
      .subscribe(() => this.load());
  }
  editDefinition(item: ResDefinition): void {
    const name = prompt(
      this.translate.instant('resource.definitionNamePrompt'),
      item.name,
    );
    if (!name) return;
    this.client.resourceConfiguration
      .updateDefinition(item.id, {
        name,
        icon: item.icon,
        properties: item.properties.map((property) => ({
          id: property.id,
          name: property.name,
          valueType: property.valueType,
          isRequired: property.isRequired,
          maxLength: property.maxLength,
          sort: property.sort,
        })),
      })
      .subscribe(() => this.load());
  }
  deleteDefinition(item: ResDefinition): void {
    if (
      confirm(
        this.translate.instant('resource.deleteDefinitionConfirm', {
          name: item.name,
        }),
      )
    )
      this.client.resourceConfiguration
        .deleteDefinition(item.id)
        .subscribe(() => this.load());
  }
  loadPermissions(): void {
    if (!this.permissionEnvironmentId || !this.permissionCategoryId) return;
    this.client.resourceConfiguration
      .permissions(this.permissionEnvironmentId, this.permissionCategoryId)
      .subscribe(
        (value) => (this.permissionRoleIds = value.map((item) => item.roleId)),
      );
  }
  savePermissions(): void {
    if (!this.permissionEnvironmentId || !this.permissionCategoryId) return;
    this.client.resourceConfiguration
      .setPermissions({
        environmentId: this.permissionEnvironmentId,
        categoryId: this.permissionCategoryId,
        roleIds: this.permissionRoleIds,
      })
      .subscribe(() =>
        this.snackBar.open(
          this.translate.instant('resource.permissionSaveSuccess'),
          this.translate.instant('common.close'),
          { duration: 2500 },
        ),
      );
  }
}
