import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ArticleCategoryItemDto } from 'src/app/services/admin/models/cmsmod/article-category-item-dto.model';
import { LanguageType } from 'src/app/services/admin/models/entity/language-type.model';
import { ContentType } from 'src/app/services/admin/models/entity/content-type.model';
import { TranslateService } from '@ngx-translate/core';
import { MarkdownEditorComponent } from 'src/app/modules/share/components/markdown-editor/markdown-editor.component';

@Component({
  selector: 'app-article-add',
  imports: [...CommonFormModules, MarkdownEditorComponent],
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleAddComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly categories = signal<ArticleCategoryItemDto[]>([]);
  readonly languages = [
    { value: LanguageType.CN, labelKey: I18N_KEYS.cms.languageTypes.CN },
    { value: LanguageType.EN, labelKey: I18N_KEYS.cms.languageTypes.EN },
  ] as const;
  readonly types = [
    { value: ContentType.News, labelKey: I18N_KEYS.cms.contentTypes.News },
    { value: ContentType.ViewPoint, labelKey: I18N_KEYS.cms.contentTypes.ViewPoint },
    { value: ContentType.Knowledge, labelKey: I18N_KEYS.cms.contentTypes.Knowledge },
    { value: ContentType.Documentary, labelKey: I18N_KEYS.cms.contentTypes.Documentary },
    { value: ContentType.Private, labelKey: I18N_KEYS.cms.contentTypes.Private },
  ] as const;
  saving = false;
  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    content: ['', Validators.required],
    languageType: LanguageType.CN,
    blogType: ContentType.News,
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
        translateTitle: null,
        translateContent: null,
      })
      .subscribe({
        next: (article) => {
          this.snackBar.open(
            this.translate.instant(this.i18nKeys.cms.article.createSuccess),
            this.translate.instant(this.i18nKeys.common.close),
            { duration: 2500 },
          );
          this.router.navigate(['/cms/article', article.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
