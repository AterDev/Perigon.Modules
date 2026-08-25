import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ArticleDetailDto } from 'src/app/services/admin/models/cmsmod/article-detail-dto.model';
import { MarkdownComponent } from 'ngx-markdown';

@Component({
  selector: 'app-article-detail',
  imports: [...CommonListModules, MarkdownComponent],
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  readonly article = signal<ArticleDetailDto | null>(null);
  constructor() {
    this.client.article
      .detail(this.route.snapshot.paramMap.get('id')!)
      .subscribe((value) => this.article.set(value));
  }
}
