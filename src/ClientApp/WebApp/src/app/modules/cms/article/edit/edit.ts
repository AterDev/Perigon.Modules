import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ArticleCategoryItemDto } from '../../../../services/admin/models/cmsmod/article-category-item-dto.model';
import { LanguageType } from '../../../../services/admin/models/entity/language-type.model';
import { ContentType } from '../../../../services/admin/models/entity/content-type.model';

@Component({
  selector: 'app-article-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleEditComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly categories = signal<ArticleCategoryItemDto[]>([]);
  readonly languages = Object.entries(LanguageType).filter(
    ([, value]) => typeof value === 'number',
  ) as [string, LanguageType][];
  readonly types = Object.entries(ContentType).filter(
    ([, value]) => typeof value === 'number',
  ) as [string, ContentType][];
  saving = false;
  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    content: ['', Validators.required],
    authors: ['', Validators.required],
    languageType: LanguageType.CN,
    blogType: ContentType.News,
    isAudit: false,
    isPublic: true,
    isOriginal: true,
    catalogId: ['', Validators.required],
    viewCount: 0,
  });
  constructor() {
    this.client.articleCategory
      .list(null, 1, 100, null)
      .subscribe((page) => this.categories.set(page.data));
    this.client.article.detail(this.id).subscribe((value) =>
      this.form.patchValue({
        title: value.title,
        description: value.description ?? '',
        content: value.content,
        authors: value.authors,
        languageType: value.languageType,
        blogType: value.blogType,
        isAudit: value.isAudit,
        isPublic: value.isPublic,
        isOriginal: value.isOriginal,
        catalogId: value.catalogId,
        viewCount: value.viewCount,
      }),
    );
  }
  save(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    this.saving = true;
    this.client.article
      .update(this.id, { ...value, description: value.description || null })
      .subscribe({
        next: () => {
          this.snackBar.open('文章已更新', '关闭', { duration: 2500 });
          this.router.navigate(['/cms/article', this.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
