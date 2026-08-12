import { I18N_KEYS } from '../../../share/i18n-keys';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResEnvironment } from '../../../../services/admin/models/entity/res-environment.model';
import { ResCategory } from '../../../../services/admin/models/entity/res-category.model';
import { ResTag } from '../../../../services/admin/models/entity/res-tag.model';
import { SystemRole } from '../../../../services/admin/models/entity/system-role.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../share/components/confirm-dialog/confirm-dialog.component';
import { ResourceInputDialogComponent, ResourceInputDialogData } from '../../dialogs/input-dialog/input-dialog';
import { ResourceGroupDialogComponent } from '../../dialogs/group-dialog/group-dialog';
import { ResourceTagDialogComponent } from '../../dialogs/tag-dialog/tag-dialog';
import { SystemRoleItemDto } from '../../../../services/admin/models/system-mod/system-role-item-dto.model';
import { ResGroup } from '../../../../services/admin/models/entity/res-group.model';
import { resourceIconName, resourceIconStyle } from '../../shared/resource-appearance';

@Component({
  selector: 'app-resource-config-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceConfigIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly iconName = resourceIconName;
  readonly iconStyle = resourceIconStyle;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly groups = signal<ResGroup[]>([]);
  readonly tags = signal<ResTag[]>([]);
  readonly roles = signal<SystemRoleItemDto[]>([]);
  readonly categoryNames = computed(
    () => new Map(this.categories().map((item) => [item.id, item.name])),
  );
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
      .groups(null)
      .subscribe((value) => this.groups.set(value));
    this.client.resourceConfiguration
      .tags()
      .subscribe((value) => this.tags.set(value));
    this.client.systemRole.list(null, null, 1, 100, null)
      .subscribe((value) => this.roles.set(value.data));
  }
  createEnvironment(): void {
    this.openInputDialog(
      {
        title: this.translate.instant('resource.environmentNamePrompt'),
        fields: [
          { key: 'name', label: this.translate.instant('resource.environment'), required: true },
          { key: 'color', label: this.translate.instant('common.color'), type: 'color', value: '#3f51b5', required: true },
          { key: 'icon', label: this.translate.instant('common.icon'), type: 'icon', value: 'cloud' },
        ],
      },
      ({ name, color, icon }) =>
        this.client.resourceConfiguration
          .addEnvironment({ name, icon: icon || null, color })
          .subscribe(() => this.load()),
    );
  }
  editEnvironment(item: ResEnvironment): void {
    this.openInputDialog(
      {
        title: this.translate.instant('resource.environmentNamePrompt'),
        fields: [
          { key: 'name', label: this.translate.instant('resource.environment'), value: item.name, required: true },
          { key: 'color', label: this.translate.instant('common.color'), type: 'color', value: item.color, required: true },
          { key: 'icon', label: this.translate.instant('common.icon'), type: 'icon', value: item.icon ?? '' },
        ],
      },
      ({ name, color, icon }) =>
        this.client.resourceConfiguration
          .updateEnvironment(item.id, { name, icon: icon || null, color })
          .subscribe(() => this.load()),
    );
  }
  deleteEnvironment(item: ResEnvironment): void {
    this.confirmDelete(
      this.translate.instant('resource.deleteEnvironmentConfirm', {
        name: item.name,
      }),
      () =>
        this.client.resourceConfiguration
          .deleteEnvironment(item.id)
          .subscribe(() => this.load()),
    );
  }
  createCategory(): void {
    this.openInputDialog(
      {
        title: this.translate.instant('resource.addCategory'),
        fields: [
          { key: 'name', label: this.translate.instant('resource.category'), required: true },
          { key: 'catalogCode', label: this.translate.instant('resource.categoryCodePrompt'), required: true },
          { key: 'color', label: this.translate.instant('common.color'), type: 'color', value: '#009688', required: true },
          { key: 'icon', label: this.translate.instant('common.icon'), type: 'icon', value: 'category' },
        ],
      },
      ({ name, catalogCode, color, icon }) =>
        this.client.resourceConfiguration
          .addCategory({ name, catalogCode, icon: icon || null, color })
          .subscribe(() => this.load()),
    );
  }
  editCategory(item: ResCategory): void {
    this.openInputDialog(
      {
        title: this.translate.instant('resource.categoryNamePrompt'),
        fields: [
          { key: 'name', label: this.translate.instant('resource.category'), value: item.name, required: true },
          { key: 'catalogCode', label: this.translate.instant('resource.categoryCodePrompt'), value: item.catalogCode, required: true },
          { key: 'color', label: this.translate.instant('common.color'), type: 'color', value: item.color, required: true },
          { key: 'icon', label: this.translate.instant('common.icon'), type: 'icon', value: item.icon ?? '' },
        ],
      },
      ({ name, catalogCode, color, icon }) =>
        this.client.resourceConfiguration
          .updateCategory(item.id, { name, catalogCode, icon: icon || null, color })
          .subscribe(() => this.load()),
    );
  }
  createGroup(): void {
    if (this.categories().length === 0) return;
    this.dialog
      .open(ResourceGroupDialogComponent, {
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { categories: this.categories() },
      })
      .afterClosed()
      .subscribe((value) => {
        if (value) this.client.resourceConfiguration.addGroup(value).subscribe(() => this.load());
      });
  }
  editGroup(item: ResGroup): void {
    this.dialog
      .open(ResourceGroupDialogComponent, {
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { group: item, categories: this.categories() },
      })
      .afterClosed()
      .subscribe((value) => {
        if (value) this.client.resourceConfiguration.updateGroup(item.id, value).subscribe(() => this.load());
      });
  }
  deleteGroup(item: ResGroup): void {
    this.confirmDelete(
      this.translate.instant('resource.deleteGroupConfirm', { name: item.name }),
      () => this.client.resourceConfiguration.deleteGroup(item.id).subscribe(() => this.load()),
    );
  }
  deleteCategory(item: ResCategory): void {
    this.confirmDelete(
      this.translate.instant('resource.deleteCategoryConfirm', {
        name: item.name,
      }),
      () =>
        this.client.resourceConfiguration
          .deleteCategory(item.id)
          .subscribe(() => this.load()),
    );
  }
  createTag(): void {
    this.dialog
      .open(ResourceTagDialogComponent, {
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((value) => {
        if (value) this.client.resourceConfiguration.addTag(value).subscribe(() => this.load());
      });
  }
  editTag(item: ResTag): void {
    this.dialog
      .open(ResourceTagDialogComponent, {
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { tag: item },
      })
      .afterClosed()
      .subscribe((value) => {
        if (value) this.client.resourceConfiguration.updateTag(item.id, value).subscribe(() => this.load());
      });
  }
  deleteTag(item: ResTag): void {
    this.confirmDelete(
      this.translate.instant('resource.deleteTagConfirm', {
        name: item.name,
      }),
      () =>
        this.client.resourceConfiguration
          .deleteTag(item.id)
          .subscribe(() => this.load()),
    );
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

  private confirmDelete(content: string, onConfirm: () => void): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant('common.confirmDelete'),
          content,
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) onConfirm();
      });
  }

  private openInputDialog(
    data: ResourceInputDialogData,
    onSubmit: (value: Record<string, string>) => void,
  ): void {
    this.dialog
      .open(ResourceInputDialogComponent, { data })
      .afterClosed()
      .subscribe((value: Record<string, string> | undefined) => {
        if (value) onSubmit(value);
      });
  }
}
