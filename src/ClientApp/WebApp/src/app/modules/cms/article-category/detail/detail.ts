import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonListModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ArticleCategoryDetailDto } from '../../../../services/admin/models/cmsmod/article-category-detail-dto.model';

@Component({
  selector: 'app-article-category-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleCategoryDetailComponent {
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly category = signal<ArticleCategoryDetailDto | null>(null);
  constructor() {
    this.client.articleCategory
      .detail(this.id)
      .subscribe((value) => this.category.set(value));
  }
}
