import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonFormModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemRoleItemDto } from '../../../../services/admin/models/system-mod/system-role-item-dto.model';
import { GenderType } from '../../../../services/admin/models/perigon/gender-type.model';
@Component({
  selector: 'app-system-user-add',
  imports: CommonFormModules,
  templateUrl: './add.html',
  styleUrl: './add.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemUserAddComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  readonly roles = signal<SystemRoleItemDto[]>([]);
  readonly genders = [
    { value: GenderType.Male, label: '男' },
    { value: GenderType.Female, label: '女' },
    { value: GenderType.Else, label: '其他' },
  ];
  saving = false;
  readonly form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(6)]],
    roleIds: [[] as string[]],
    realName: [''],
    email: ['', Validators.email],
    phoneNumber: [''],
    avatar: [''],
    sex: GenderType.Else,
  });

  constructor() {
    this.client.systemRole
      .list(null, null, 1, 100, null)
      .subscribe((page) => this.roles.set(page.data));
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.client.systemUser.add(this.form.getRawValue()).subscribe({
      next: () => {
        this.snackBar.open('账号已创建', '关闭', { duration: 2500 });
        this.router.navigate(['/system/user']);
      },
      error: () => (this.saving = false),
    });
  }
}
