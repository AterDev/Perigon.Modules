import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemRoleDetailDto } from 'src/app/services/admin/models/system-mod/system-role-detail-dto.model';
import { MatDialog } from '@angular/material/dialog';
import { SystemRoleEditComponent } from 'src/app/modules/system/role/edit/edit';

@Component({
  selector: 'app-system-role-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemRoleDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly role = signal<SystemRoleDetailDto | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.client.systemRole.detail(this.id).subscribe((value) => this.role.set(value));
  }

  edit(): void {
    this.dialog
      .open(SystemRoleEditComponent, {
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
