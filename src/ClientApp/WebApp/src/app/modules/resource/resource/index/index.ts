import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResourceItemDto } from '../../../../services/admin/models/resource-mod/resource-item-dto.model';
import { ResEnvironment } from '../../../../services/admin/models/entity/res-environment.model';
import { ResCategory } from '../../../../services/admin/models/entity/res-category.model';
import { ResDefinition } from '../../../../services/admin/models/entity/res-definition.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../share/components/confirm-dialog/confirm-dialog.component';
import { ResourceAddComponent } from '../add/add';
import { ResourceDetailComponent } from '../detail/detail';
import { ResourceEditComponent } from '../edit/edit';

@Component({
  selector: 'app-resource-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);
  readonly resources = signal<ResourceItemDto[]>([]);
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly definitions = signal<ResDefinition[]>([]);
  readonly loading = signal(false);
  environmentId = '';
  categoryId = '';
  definitionId = '';
  tagName = '';

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
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.resource
      .list(
        this.environmentId || null,
        this.categoryId || null,
        null,
        this.definitionId || null,
        this.tagName || null,
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
