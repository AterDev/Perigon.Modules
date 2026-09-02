import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { UserResourceItemDto } from 'src/app/services/admin/models/resource-mod/user-resource-item-dto.model';
import { UserResourceReviewDialogComponent } from 'src/app/modules/resource/personal-resource/review/review-dialog';

@Component({
  selector: 'app-user-resource-review',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserResourceReviewComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly resources = signal<UserResourceItemDto[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal(false);
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.client.userResource.review(null, null, 1, 100, null).subscribe({
      next: (page) => {
        this.resources.set(page.data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
      },
    });
  }

  review(resource: UserResourceItemDto): void {
    this.dialog
      .open(UserResourceReviewDialogComponent, {
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
