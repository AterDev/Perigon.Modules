import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { TranslatePipe } from '@ngx-translate/core';
import { finalize, switchMap } from 'rxjs';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { NotificationService } from 'src/app/services/admin/services/notification.service';
import { NotificationActions } from '../shared/notification-actions';
import { NotificationListComponent } from '../shared/notification-list';
import { createNotificationPage } from '../shared/notification-page';

@Component({
  selector: 'app-notification-deliveries',
  imports: [
    DatePipe,
    TranslatePipe,
    MatButton,
    MatIcon,
    NotificationListComponent,
  ],
  templateUrl: './deliveries.html',
  styleUrl: '../shared/notification-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationDeliveriesComponent {
  private readonly notifications = inject(NotificationService);
  private readonly actions = inject(NotificationActions);
  private readonly destroyRef = inject(DestroyRef);
  readonly notificationKeys = I18N_KEYS.notification;
  readonly list = createNotificationPage((page) =>
    this.notifications.deliveries(page),
  );
  readonly recovering = signal(false);

  constructor() {
    this.list.load();
  }

  recover(): void {
    if (this.recovering()) return;
    this.recovering.set(true);
    this.actions
      .confirm(
        this.notificationKeys.recover,
        this.notificationKeys.recoverConfirm,
      )
      .pipe(
        switchMap(() => this.notifications.recover()),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.recovering.set(false)),
      )
      .subscribe({
        next: (result) => {
          this.actions.notify(this.notificationKeys.recoverSuccess, result);
          this.list.load();
        },
        error: () => this.actions.notify(I18N_KEYS.common.saveFail),
      });
  }
}
