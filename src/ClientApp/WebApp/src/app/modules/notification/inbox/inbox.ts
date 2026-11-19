import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { InAppNotificationDetailDto } from 'src/app/services/admin/models/notification-mod/in-app-notification-detail-dto.model';
import { InAppNotificationItemDto } from 'src/app/services/admin/models/notification-mod/in-app-notification-item-dto.model';

@Component({
  selector: 'app-notification-inbox',
  imports: CommonListModules,
  templateUrl: './inbox.html',
  styleUrl: './inbox.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationInboxComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly notificationKeys = I18N_KEYS.notification;
  readonly notifications = signal<InAppNotificationItemDto[]>([]);
  readonly unreadCount = signal(0);
  readonly selected = signal<InAppNotificationDetailDto | null>(null);
  readonly loading = signal(false);
  readonly detailLoading = signal(false);
  readonly loadError = signal(false);
  readonly detailError = signal(false);
  readonly markingRead = signal(new Set<string>());
  readonly total = signal(0);
  pageIndex = 0;
  pageSize = 20;
  searchTitle = '';
  private readonly client = inject(AdminClient);
  private listRequest?: Subscription;
  private detailRequest?: Subscription;

  constructor() {
    this.load();
  }

  load(): void {
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.loadError.set(false);
    this.listRequest = this.client.notification
      .mine({
        title: this.searchTitle.trim() || undefined,
        pageIndex: this.pageIndex + 1,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (page) => {
          this.notifications.set(page.data);
          this.total.set(page.count);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.loadError.set(true);
        },
      });
    this.refreshUnread();
  }

  search(): void {
    this.pageIndex = 0;
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  open(item: InAppNotificationItemDto): void {
    this.detailRequest?.unsubscribe();
    this.detailLoading.set(true);
    this.detailError.set(false);
    this.selected.set(null);
    this.detailRequest = this.client.notification
      .mineDetail(item.id)
      .subscribe({
        next: (detail) => {
          this.selected.set(detail);
          this.detailLoading.set(false);
          if (!detail.isRead) this.markRead(item);
        },
        error: () => {
          this.detailLoading.set(false);
          this.detailError.set(true);
        },
      });
  }

  markRead(item: InAppNotificationItemDto): void {
    if (item.isRead || this.markingRead().has(item.id)) return;
    this.markingRead.update((ids) => new Set(ids).add(item.id));
    this.client.notification.read(item.id).subscribe({
      next: () => {
        this.notifications.update((items) =>
          items.map((value) =>
            value.id === item.id ? { ...value, isRead: true } : value,
          ),
        );
        this.selected.update((value) =>
          value && value.id === item.id ? { ...value, isRead: true } : value,
        );
        this.finishRead(item.id);
        this.refreshUnread();
      },
      error: () => this.finishRead(item.id),
    });
  }

  private refreshUnread(): void {
    this.client.notification.unreadCount().subscribe({
      next: (count) => this.unreadCount.set(count),
      error: () => undefined,
    });
  }

  private finishRead(id: string): void {
    this.markingRead.update((ids) => {
      const remaining = new Set(ids);
      remaining.delete(id);
      return remaining;
    });
  }

  closeDetail(): void {
    this.selected.set(null);
  }
}
