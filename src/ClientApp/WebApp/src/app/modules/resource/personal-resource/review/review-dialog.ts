import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { finalize, forkJoin } from 'rxjs';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';
import { ResEnvironment } from 'src/app/services/admin/models/entity/res-environment.model';
import { ResGroup } from 'src/app/services/admin/models/entity/res-group.model';
import { ResTag } from 'src/app/services/admin/models/entity/res-tag.model';
import { PersonalResourceDetailDto } from 'src/app/services/admin/models/resource-mod/personal-resource-detail-dto.model';

@Component({
  selector: 'app-personal-resource-review-dialog',
  imports: CommonFormModules,
  templateUrl: './review-dialog.html',
  styleUrl: './review-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonalResourceReviewDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly detail = signal<PersonalResourceDetailDto | null>(null);
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly groups = signal<ResGroup[]>([]);
  readonly tags = signal<ResTag[]>([]);
  readonly loading = signal(true);
  readonly form = inject(FormBuilder).nonNullable.group({
    environmentId: ['', Validators.required],
    categoryId: ['', Validators.required],
    groupId: [''],
    tagNames: [[] as string[]],
    reviewComment: [''],
  });
  private readonly client = inject(AdminClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly dialogRef = inject(MatDialogRef<PersonalResourceReviewDialogComponent>);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  saving = false;

  constructor() {
    forkJoin({
      detail: this.client.personalResource.detail(this.data.id),
      environments: this.client.resourceConfiguration.environments(),
      categories: this.client.resourceConfiguration.categories(),
      tags: this.client.resourceConfiguration.tags(),
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((result) => {
        this.detail.set(result.detail);
        this.environments.set(result.environments);
        this.categories.set(result.categories);
        this.tags.set(result.tags);
        this.form.patchValue({
          environmentId: result.environments[0]?.id ?? '',
          categoryId: result.categories[0]?.id ?? '',
        });
        this.categoryChanged();
      });
  }

  categoryChanged(): void {
    const categoryId = this.form.controls.categoryId.value;
    this.form.controls.groupId.setValue('');
    if (!categoryId) {
      this.groups.set([]);
      return;
    }
    this.client.resourceConfiguration.groups(categoryId).subscribe((groups) => this.groups.set(groups));
  }

  approve(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.personalResource.approve(this.data.id, this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant('resource.personalApproveSuccess'),
          this.translate.instant('common.close'),
          { duration: 2500 },
        );
        this.dialogRef.close(true);
      },
      error: () => (this.saving = false),
    });
  }

  reject(): void {
    this.saving = true;
    this.client.personalResource
      .reject(this.data.id, { reviewComment: this.form.controls.reviewComment.value })
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('resource.personalRejectSuccess'),
            this.translate.instant('common.close'),
            { duration: 2500 },
          );
          this.dialogRef.close(true);
        },
        error: () => (this.saving = false),
      });
  }
}
