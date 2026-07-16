import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { CommonListModules } from '../../../share/shared-modules';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResourceDetailDto } from '../../../../services/admin/models/resource-mod/resource-detail-dto.model';
import { ResourceEditComponent } from '../edit/edit';

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
  private readonly route = inject(ActivatedRoute, { optional: true });
  private readonly dialog = inject(MatDialog);
  private readonly data = inject<{ id: string }>(MAT_DIALOG_DATA, { optional: true });
  readonly id = this.data?.id ?? this.route?.snapshot.paramMap.get('id') ?? '';
  readonly resource = signal<ResourceDetailDto | null>(null);
  constructor() {
    this.load();
  }

  load(): void {
    this.client.resource.detail(this.id).subscribe((value) => this.resource.set(value));
  }

  edit(): void {
    this.dialog
      .open(ResourceEditComponent, {
        width: '900px',
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
