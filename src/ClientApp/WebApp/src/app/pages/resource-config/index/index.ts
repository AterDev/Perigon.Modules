import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonListModules } from '../../../share/shared-modules';

interface NamedResource { id: string; name: string; icon?: string; color?: string; }

@Component({
  selector: 'app-resource-config-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceConfigIndex {
  private readonly http = inject(HttpClient);
  readonly environments = signal<NamedResource[]>([]);
  readonly categories = signal<NamedResource[]>([]);
  readonly tags = signal<NamedResource[]>([]);

  constructor() { this.load(); }

  load(): void {
    this.http.get<NamedResource[]>('/api/ResourceConfiguration/environments').subscribe(value => this.environments.set(value));
    this.http.get<NamedResource[]>('/api/ResourceConfiguration/categories').subscribe(value => this.categories.set(value));
    this.http.get<NamedResource[]>('/api/ResourceConfiguration/tags').subscribe(value => this.tags.set(value));
  }
}
