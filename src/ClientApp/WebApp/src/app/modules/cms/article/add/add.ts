import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ArticleCategoryItemDto } from '../../../../services/admin/models/cmsmod/article-category-item-dto.model';
import { LanguageType } from '../../../../services/admin/models/entity/language-type.model';
import { ContentType } from '../../../../services/admin/models/entity/content-type.model';
import { AuthService } from '../../../../services/auth.service';

@Component({
  selector: 'app-article-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleAddComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
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
  });
  constructor() {
    this.client.articleCategory
      .list(null, 1, 100, null)
      .subscribe((page) => this.categories.set(page.data));
  }
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving = true;
    this.client.article
      .add({
        ...value,
        description: value.description || null,
        userId: this.auth.id ?? '00000000-0000-0000-0000-000000000000',
        viewCount: 0,
        translateTitle: null,
        translateContent: null,
      })
      .subscribe({
        next: (article) => {
          this.snackBar.open('文章已创建', '关闭', { duration: 2500 });
          this.router.navigate(['/cms/article', article.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
