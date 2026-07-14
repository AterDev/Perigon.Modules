import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonListModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemPermissionDetailDto } from '../../../../services/admin/models/system-mod/system-permission-detail-dto.model';
@Component({
  selector: 'app-system-permission-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemPermissionDetailComponent {
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  readonly permission = signal<SystemPermissionDetailDto | null>(null);
  constructor() {
    this.client.systemPermission
      .getDetail(this.route.snapshot.paramMap.get('id')!)
      .subscribe((value) => this.permission.set(value));
  }
}
