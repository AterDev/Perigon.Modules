import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ResEnvironment } from 'src/app/services/admin/models/entity/res-environment.model';
import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';
import { SystemRole } from 'src/app/services/admin/models/entity/system-role.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { ResourceInputDialogComponent, ResourceInputDialogData } from 'src/app/modules/resource/dialogs/input-dialog/input-dialog';
import { SystemRoleItemDto } from 'src/app/services/admin/models/system-mod/system-role-item-dto.model';
import { resourceIconName, resourceIconStyle } from 'src/app/modules/resource/shared/resource-appearance';
import { forkJoin } from 'rxjs';

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
  readonly roles = signal<SystemRoleItemDto[]>([]);
  permissionEnvironmentId = '';
  permissionCategoryId = '';
  permissionRoleIds: string[] = [];
  constructor() {
    this.load();
  }

  load(): void {
    forkJoin({
      environments: this.client.resourceConfiguration.environments(),
      categories: this.client.resourceConfiguration.categories(),
      roles: this.client.systemRole.list(null, null, 1, 100, null),
    }).subscribe((result) => {
      this.environments.set(result.environments);
      this.categories.set(result.categories);
      this.roles.set(result.roles.data);
    });
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
