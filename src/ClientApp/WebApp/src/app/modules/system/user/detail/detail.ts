import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SystemUserDetailDto } from 'src/app/services/admin/models/system-mod/system-user-detail-dto.model';
import { MatDialog } from '@angular/material/dialog';
import { SystemUserEditComponent } from 'src/app/modules/system/user/edit/edit';

@Component({
  selector: 'app-system-user-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemUserDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly user = signal<SystemUserDetailDto | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.client.systemUser.getDetail(this.id).subscribe((value) => this.user.set(value));
  }

  edit(): void {
    this.dialog
      .open(SystemUserEditComponent, {
        width: '640px',
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
