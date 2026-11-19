import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatPaginator } from '@angular/material/paginator';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { TranslatePipe } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { NotificationPage } from './notification-page';

@Component({
  selector: 'app-notification-list',
  imports: [MatButton, MatPaginator, MatProgressSpinner, TranslatePipe],
  templateUrl: './notification-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationListComponent {
  readonly list = input.required<NotificationPage<unknown>>();
  readonly label = input.required<string>();
  readonly i18nKeys = I18N_KEYS;
}
