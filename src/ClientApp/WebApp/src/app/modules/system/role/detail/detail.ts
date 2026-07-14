import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonListModules } from '../../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { SystemRoleDetailDto } from '../../../../services/admin/models/system-mod/system-role-detail-dto.model';
@Component({
  selector: 'app-system-role-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemRoleDetailComponent {
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  readonly role = signal<SystemRoleDetailDto | null>(null);
  constructor() {
    this.client.systemRole
      .detail(this.route.snapshot.paramMap.get('id')!)
      .subscribe((value) => this.role.set(value));
  }
}
