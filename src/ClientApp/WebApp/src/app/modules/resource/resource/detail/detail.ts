import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResourceDetailDto } from '../../../../services/admin/models/resource-mod/resource-detail-dto.model';

@Component({
  selector: 'app-resource-detail',
  imports: CommonListModules,
  templateUrl: './detail.html',
  styleUrl: './detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceDetailComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly route = inject(ActivatedRoute);
  readonly resource = signal<ResourceDetailDto | null>(null);
  constructor() {
    this.client.resource
      .detail(this.route.snapshot.paramMap.get('id')!)
      .subscribe((value) => this.resource.set(value));
  }
}
