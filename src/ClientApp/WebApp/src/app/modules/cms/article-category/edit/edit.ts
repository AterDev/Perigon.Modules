import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';

@Component({
  selector: 'app-article-category-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleCategoryEditComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  saving = false;
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
  });
  constructor() {
    this.client.articleCategory
      .detail(this.id)
      .subscribe((value) => this.form.patchValue({ name: value.name }));
  }
  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.client.articleCategory
      .update(this.id, this.form.getRawValue())
      .subscribe({
        next: () => {
          this.snackBar.open('分类已更新', '关闭', { duration: 2500 });
          this.router.navigate(['/cms/article-category', this.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
