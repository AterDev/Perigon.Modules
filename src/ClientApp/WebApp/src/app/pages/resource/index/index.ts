import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CommonListModules } from '../../../share/shared-modules';

interface ResourceItem {
  id: string;
  name: string;
  environmentName: string;
  categoryName: string;
  groupName?: string;
  definitionName: string;
  tagNames: string[];
}

interface PageList<T> { data: T[]; count: number; pageIndex: number; }

@Component({
  selector: 'app-resource-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceIndex {
  private readonly http = inject(HttpClient);
  readonly resources = signal<ResourceItem[]>([]);
  readonly loading = signal(false);
  readonly keyword = signal('');

  constructor() { this.load(); }

  load(): void {
    this.loading.set(true);
    const params = new HttpParams().set('pageIndex', 1).set('pageSize', 20).set('name', this.keyword());
    this.http.get<PageList<ResourceItem>>('/api/Resource/list', { params }).subscribe({
      next: page => { this.resources.set(page.data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
