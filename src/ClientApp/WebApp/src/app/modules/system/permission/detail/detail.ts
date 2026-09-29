import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SysDataScopeDetailDto } from 'src/app/services/admin/models/system-mod/sys-data-scope-detail-dto.model';
import { MatDialog } from '@angular/material/dialog';
import { SysDataScopeEditComponent } from 'src/app/modules/system/permission/edit/edit';
import { DataScopeType } from 'src/app/services/admin/models/entity/data-scope-type.model';

@Component({
  selector: 'app-sys-data-scope-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysDataScopeDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly dataScope = signal<SysDataScopeDetailDto | null>(null);
  readonly groupName = signal<string | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.client.sysDataScope.getDetail(this.id).subscribe((value) => {
      this.dataScope.set(value);
      this.client.sysDataScopeGroup.getDetail(value.groupId).subscribe((group) => {
        this.groupName.set(group.name);
      });
    });
  }

  scopeTypeKey(scopeType: DataScopeType): string {
    switch (scopeType) {
      case DataScopeType.None:
        return this.i18nKeys.sysDataScope.types.none;
      case DataScopeType.All:
        return this.i18nKeys.sysDataScope.types.all;
      case DataScopeType.Include:
        return this.i18nKeys.sysDataScope.types.include;
    }
    return this.i18nKeys.sysDataScope.types.none;
  }

  edit(): void {
    this.dialog
      .open(SysDataScopeEditComponent, {
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
