import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';

@Component({
  selector: 'app-index',
  imports: [TranslateModule],
  templateUrl: './index.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './index.scss',
})
export class Index {
  i18nKeys = I18N_KEYS;
}
