import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemPermissionDetailDto } from 'src/app/services/admin/models/system-mod/system-permission-detail-dto.model';
import { MatDialog } from '@angular/material/dialog';
import { SystemPermissionEditComponent } from 'src/app/modules/system/permission/edit/edit';

@Component({
  selector: 'app-system-permission-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemPermissionDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly permission = signal<SystemPermissionDetailDto | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.client.systemPermission.getDetail(this.id).subscribe((value) => this.permission.set(value));
  }

  edit(): void {
    this.dialog
      .open(SystemPermissionEditComponent, {
        width: '620px',
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
