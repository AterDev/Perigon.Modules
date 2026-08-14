import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ResourceItemDto } from 'src/app/services/admin/models/resource-mod/resource-item-dto.model';
import { ResEnvironment } from 'src/app/services/admin/models/entity/res-environment.model';
import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';
import { ResDefinition } from 'src/app/services/admin/models/entity/res-definition.model';
import { ResGroup } from 'src/app/services/admin/models/entity/res-group.model';
import { ResTag } from 'src/app/services/admin/models/entity/res-tag.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { ResourceAddComponent } from 'src/app/modules/resource/resource/add/add';
import { ResourceDetailComponent } from 'src/app/modules/resource/resource/detail/detail';
import { ResourceEditComponent } from 'src/app/modules/resource/resource/edit/edit';
import { resourceIconName, resourceIconStyle } from 'src/app/modules/resource/shared/resource-appearance';

@Component({
  selector: 'app-resource-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly iconName = resourceIconName;
  readonly iconStyle = resourceIconStyle;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);
  readonly resources = signal<ResourceItemDto[]>([]);
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly definitions = signal<ResDefinition[]>([]);
  readonly groups = signal<ResGroup[]>([]);
  readonly tags = signal<ResTag[]>([]);
  readonly environmentsById = computed(
    () => new Map(this.environments().map((item) => [item.id, item])),
  );
  readonly categoriesById = computed(
    () => new Map(this.categories().map((item) => [item.id, item])),
  );
  readonly groupsById = computed(
    () => new Map(this.groups().map((item) => [item.id, item])),
  );
  readonly definitionsById = computed(
    () => new Map(this.definitions().map((item) => [item.id, item])),
  );
  readonly tagsByName = computed(
    () => new Map(this.tags().map((item) => [item.name, item])),
  );
  readonly loading = signal(false);
  environmentId = '';
  categoryId = '';
  definitionId = '';
  tagName = '';
  searchKey = '';

  constructor() {
    this.client.resourceConfiguration
      .environments()
      .subscribe((value) => this.environments.set(value));
    this.client.resourceConfiguration
      .categories()
      .subscribe((value) => this.categories.set(value));
    this.client.resourceConfiguration
      .definitions(null)
      .subscribe((value) => this.definitions.set(value));
    this.client.resourceConfiguration
      .groups(null)
      .subscribe((value) => this.groups.set(value));
    this.client.resourceConfiguration
      .tags()
      .subscribe((value) => this.tags.set(value));
    this.load();
  }

  load(): void {
    const searchKey = this.searchKey.trim();
    if (searchKey.length === 1) return;

    this.loading.set(true);
    this.client.resource
      .list(
        this.environmentId || null,
        this.categoryId || null,
        null,
        this.definitionId || null,
        this.tagName || null,
        searchKey || null,
        1,
        50,
        null,
      )
      .subscribe({
        next: (page) => {
          this.resources.set(page.data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  add(): void {
    this.dialog
      .open(ResourceAddComponent, {
        width: '900px',
        maxWidth: '96vw',
        maxHeight: '96vh',
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  detail(resource: ResourceItemDto): void {
    this.dialog.open(ResourceDetailComponent, {
      width: '760px',
      maxWidth: '96vw',
      maxHeight: '96vh',
      data: { id: resource.id },
    });
  }

  edit(resource: ResourceItemDto): void {
    this.dialog
      .open(ResourceEditComponent, {
        width: '900px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: resource.id },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }

  remove(resource: ResourceItemDto): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant('common.confirmDelete'),
          content:
        this.translate.instant('resource.deleteResourceConfirm', {
          name: resource.definitionName,
        }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.resource.delete(resource.id).subscribe(() => {
          this.snackBar.open(
            this.translate.instant('resource.deleteSuccess'),
            this.translate.instant('common.close'),
            { duration: 2500 },
          );
          this.load();
        });
      });
  }
}
