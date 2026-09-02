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
import { UserResourceDetailDto } from 'src/app/services/admin/models/resource-mod/user-resource-detail-dto.model';
import { UserResourceRejectDto } from 'src/app/services/admin/models/resource-mod/user-resource-reject-dto.model';
import { UserResourceReviewDto } from 'src/app/services/admin/models/resource-mod/user-resource-review-dto.model';

@Component({
  selector: 'app-user-resource-review-dialog',
  imports: CommonFormModules,
  templateUrl: './review-dialog.html',
  styleUrl: './review-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserResourceReviewDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly detail = signal<UserResourceDetailDto | null>(null);
  readonly environments = signal<ResEnvironment[]>([]);
  readonly categories = signal<ResCategory[]>([]);
  readonly groups = signal<ResGroup[]>([]);
  readonly tags = signal<ResTag[]>([]);
  readonly loading = signal(true);
  readonly groupsLoading = signal(false);
  readonly loadError = signal(false);
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
  private readonly dialogRef = inject(MatDialogRef<UserResourceReviewDialogComponent>);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA);
  saving = false;

  constructor() {
    forkJoin({
      detail: this.client.userResource.detail(this.data.id),
      environments: this.client.resourceConfiguration.environments(),
      categories: this.client.resourceConfiguration.categories(),
      tags: this.client.resourceConfiguration.tags(),
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          this.detail.set(result.detail);
          this.environments.set(result.environments);
          this.categories.set(result.categories);
          this.tags.set(result.tags);
          this.form.patchValue({
            environmentId: result.environments[0]?.id ?? '',
            categoryId: result.categories[0]?.id ?? '',
          });
          this.categoryChanged();
        },
        error: () => this.loadError.set(true),
      });
  }

  categoryChanged(): void {
    const categoryId = this.form.controls.categoryId.value;
    this.form.controls.groupId.setValue('');
    this.groups.set([]);
    this.groupsLoading.set(false);
    if (!categoryId) return;

    this.groupsLoading.set(true);
    this.client.resourceConfiguration.groups(categoryId).pipe(
      finalize(() => this.groupsLoading.set(false)),
    ).subscribe({
      next: (groups) => this.groups.set(groups),
      error: () => this.loadError.set(true),
    });
  }

  approve(): void {
    if (this.form.invalid || this.groupsLoading() || this.loadError()) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    const formValue = this.form.getRawValue();
    const payload: UserResourceReviewDto = {
      environmentId: formValue.environmentId,
      categoryId: formValue.categoryId,
      groupId: formValue.groupId || null,
      tagNames: formValue.tagNames,
      reviewComment: formValue.reviewComment || null,
    };
    this.client.userResource.approve(this.data.id, payload).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.resource.userApproveSuccess),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2500 },
        );
        this.dialogRef.close(true);
      },
      error: () => this.saveFailed(),
    });
  }

  reject(): void {
    if (this.loadError()) return;
    this.saving = true;
    const payload: UserResourceRejectDto = {
      reviewComment: this.form.controls.reviewComment.value || null,
    };
    this.client.userResource.reject(this.data.id, payload).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant(this.i18nKeys.resource.userRejectSuccess),
          this.translate.instant(this.i18nKeys.common.close),
          { duration: 2500 },
        );
        this.dialogRef.close(true);
      },
      error: () => this.saveFailed(),
    });
  }

  private saveFailed(): void {
    this.saving = false;
    this.snackBar.open(
      this.translate.instant(this.i18nKeys.common.saveFail),
      this.translate.instant(this.i18nKeys.common.close),
      { duration: 2500 },
    );
  }
}
