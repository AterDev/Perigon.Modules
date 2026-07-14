import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { CommonListModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResourceItemDto } from '../../../../services/admin/models/resource-mod/resource-item-dto.model';
import { ResEnvironment } from '../../../../services/admin/models/entity/res-environment.model';
import { ResCategory } from '../../../../services/admin/models/entity/res-category.model';
import { ResDefinition } from '../../../../services/admin/models/entity/res-definition.model';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-resource-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceIndexComponent {
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  readonly resources = signal<ResourceItemDto[]>([]);
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly definitions = signal<ResDefinition[]>([]);
  readonly loading = signal(false);
  name = '';
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
      .definitions()
      .subscribe((value) => this.definitions.set(value));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.resource
      .list(
        this.name || null,
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

  remove(resource: ResourceItemDto): void {
    if (!confirm(`确定删除资源“${resource.name}”吗？`)) return;
    this.client.resource.delete(resource.id).subscribe(() => {
      this.snackBar.open('资源已删除', '关闭', { duration: 2500 });
      this.load();
    });
  }
}
