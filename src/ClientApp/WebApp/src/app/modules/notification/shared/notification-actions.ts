import { inject, Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { filter, Observable } from 'rxjs';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';

@Injectable({ providedIn: 'root' })
export class NotificationActions {
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  confirm(
    titleKey: string,
    bodyKey: string,
    params?: object,
  ): Observable<boolean> {
    const dialog = this.dialog.open<
      ConfirmDialogComponent,
      { title: string; content: string },
      boolean
    >(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant(titleKey),
        content: this.translate.instant(bodyKey, params),
      },
    });
    return dialog.afterClosed().pipe(filter((confirmed) => confirmed === true));
  }

  confirmDelete(): Observable<boolean> {
    const key = I18N_KEYS.common.confirmDelete;
    return this.confirm(key, key);
  }

  notify(key: string, params?: object): void {
    this.snackBar.open(
      this.translate.instant(key, params),
      this.translate.instant(I18N_KEYS.common.close),
      { duration: 2500 },
    );
  }
}
