import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { NotificationService } from 'src/app/services/admin/services/notification.service';
import { NotificationListComponent } from '../shared/notification-list';
import { createNotificationPage } from '../shared/notification-page';

@Component({
  selector: 'app-notification-events',
  imports: [DatePipe, TranslatePipe, NotificationListComponent],
  templateUrl: './events.html',
  styleUrl: '../shared/notification-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationEventsComponent {
  private readonly notifications = inject(NotificationService);
  readonly notificationKeys = I18N_KEYS.notification;
  readonly list = createNotificationPage((page) =>
    this.notifications.events(page),
  );

  constructor() {
    this.list.load();
  }
}
