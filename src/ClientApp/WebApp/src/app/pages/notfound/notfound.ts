import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';

@Component({
  selector: 'app-notfound',
  imports: [TranslateModule],
  templateUrl: './notfound.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './notfound.css',
})
export class Notfound {
  i18nKeys = I18N_KEYS;
}
