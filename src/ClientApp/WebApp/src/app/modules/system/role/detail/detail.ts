import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SysRoleDetailDto } from 'src/app/services/admin/models/system-mod/sys-role-detail-dto.model';
import { MatDialog } from '@angular/material/dialog';
import { SysRoleEditComponent } from 'src/app/modules/system/role/edit/edit';

@Component({
  selector: 'app-sys-role-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysRoleDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly role = signal<SysRoleDetailDto | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.client.sysRole.detail(this.id).subscribe((value) => this.role.set(value));
  }

  edit(): void {
    this.dialog
      .open(SysRoleEditComponent, {
        width: '520px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: this.id },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) this.load();
      });
  }
}
