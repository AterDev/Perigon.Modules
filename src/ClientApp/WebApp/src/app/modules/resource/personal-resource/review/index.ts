import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { PersonalResourceItemDto } from 'src/app/services/admin/models/resource-mod/personal-resource-item-dto.model';
import { PersonalResourceReviewDialogComponent } from 'src/app/modules/resource/personal-resource/review/review-dialog';

@Component({
  selector: 'app-personal-resource-review',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonalResourceReviewComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly resources = signal<PersonalResourceItemDto[]>([]);
  readonly loading = signal(false);
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.client.personalResource.review({ pageIndex: 1, pageSize: 100 }).subscribe({
      next: (page) => {
        this.resources.set(page.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  review(resource: PersonalResourceItemDto): void {
    this.dialog
      .open(PersonalResourceReviewDialogComponent, {
        width: '900px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { id: resource.id },
      })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.load();
      });
  }
}
