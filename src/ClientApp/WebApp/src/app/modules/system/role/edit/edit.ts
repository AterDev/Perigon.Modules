import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
@Component({
  selector: 'app-system-role-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemRoleEditComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    nameValue: ['', Validators.required],
    isSystem: false,
  });
  saving = false;
  constructor() {
    this.client.systemRole
      .detail(this.id)
      .subscribe((value) => this.form.patchValue(value));
  }
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemRole.update(this.id, this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open('角色已更新', '关闭', { duration: 2500 });
        this.router.navigate(['/system/role', this.id, 'detail']);
      },
      error: () => (this.saving = false),
    });
  }
}
