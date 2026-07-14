import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemUserDetailDto } from '../../../../services/admin/models/system-mod/system-user-detail-dto.model';
@Component({
  selector: 'app-system-user-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemUserDetailComponent {
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  readonly user = signal<SystemUserDetailDto | null>(null);
  constructor() {
    this.client.systemUser
      .getDetail(this.route.snapshot.paramMap.get('id')!)
      .subscribe((value) => this.user.set(value));
  }
}
