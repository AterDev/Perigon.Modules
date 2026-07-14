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
import { SystemRoleItemDto } from '../../../../services/admin/models/system-mod/system-role-item-dto.model';
import { GenderType } from '../../../../services/admin/models/perigon/gender-type.model';
@Component({
  selector: 'app-system-user-edit',
  imports: CommonFormModules,
  templateUrl: './edit.html',
  styleUrl: './edit.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemUserEditComponent {
  private readonly fb = inject(FormBuilder);
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly roles = signal<SystemRoleItemDto[]>([]);
  readonly genders = [
    { value: GenderType.Male, label: '男' },
    { value: GenderType.Female, label: '女' },
    { value: GenderType.Else, label: '其他' },
  ];
  saving = false;
  readonly form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    password: [''],
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
    this.client.systemUser.getDetail(this.id).subscribe((value) =>
      this.form.patchValue({
        userName: value.userName,
        realName: value.realName ?? '',
        email: value.email ?? '',
        phoneNumber: value.phoneNumber ?? '',
        avatar: value.avatar ?? '',
        sex: value.sex,
      }),
    );
  }
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving = true;
    this.client.systemUser
      .update(this.id, { ...value, password: value.password || null })
      .subscribe({
        next: () => {
          this.snackBar.open('账号已更新', '关闭', { duration: 2500 });
          this.router.navigate(['/system/user', this.id, 'detail']);
        },
        error: () => (this.saving = false),
      });
  }
}
