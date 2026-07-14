import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
@Component({
  selector: 'app-system-role-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemRoleAddComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  saving = false;
  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    nameValue: ['', [Validators.required, Validators.maxLength(50)]],
    isSystem: false,
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemRole.add(this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open('角色已创建', '关闭', { duration: 2500 });
        this.router.navigate(['/system/role']);
      },
      error: () => (this.saving = false),
    });
  }
}
