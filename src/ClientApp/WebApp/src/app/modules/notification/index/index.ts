import {
  ChangeDetectionStrategy,
  Component,
  signal,
  viewChild,
} from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslatePipe } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { NotificationDeliveriesComponent } from '../deliveries/deliveries';
import { NotificationEventsComponent } from '../events/events';
import { NotificationMessagesComponent } from '../messages/messages';
import { NotificationReceiversComponent } from '../receivers/receivers';
import { NotificationRulesComponent } from '../rules/rules';
import { NotificationSourcesComponent } from '../sources/sources';

@Component({
  selector: 'app-notification-index',
  imports: [
    MatButton,
    MatIcon,
    MatTabsModule,
    TranslatePipe,
    NotificationMessagesComponent,
    NotificationSourcesComponent,
    NotificationReceiversComponent,
    NotificationRulesComponent,
    NotificationEventsComponent,
    NotificationDeliveriesComponent,
  ],
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly activeTab = signal(0);

  private readonly messages = viewChild(NotificationMessagesComponent);
  private readonly sources = viewChild(NotificationSourcesComponent);
  private readonly receivers = viewChild(NotificationReceiversComponent);
  private readonly rules = viewChild(NotificationRulesComponent);
  private readonly events = viewChild(NotificationEventsComponent);
  private readonly deliveries = viewChild(NotificationDeliveriesComponent);

  refresh(): void {
    const panels = [
      this.messages,
      this.sources,
      this.receivers,
      this.rules,
      this.events,
      this.deliveries,
    ];
    panels[this.activeTab()]()?.list.load();
  }
}
