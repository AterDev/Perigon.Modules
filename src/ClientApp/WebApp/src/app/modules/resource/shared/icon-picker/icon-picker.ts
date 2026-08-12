import { ChangeDetectionStrategy, Component, forwardRef, inject, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  resourceIconName,
  resourceIconStyle,
} from '../resource-appearance';
import {
  ResourceIconPickerDialogComponent,
  ResourceIconPickerDialogData,
} from './icon-picker-dialog';

@Component({
  selector: 'app-resource-icon-picker',
  imports: [MatButtonModule, MatIconModule, MatTooltipModule, TranslateModule],
  templateUrl: './icon-picker.html',
  styleUrl: './icon-picker.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ResourceIconPickerComponent),
      multi: true,
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceIconPickerComponent implements ControlValueAccessor {
  readonly i18nKeys = I18N_KEYS;
  readonly value = signal('');
  readonly disabled = signal(false);
  private readonly dialog = inject(MatDialog);
  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  open(): void {
    if (this.disabled()) return;
    this.dialog
      .open<ResourceIconPickerDialogComponent, ResourceIconPickerDialogData, string>(
        ResourceIconPickerDialogComponent,
        {
          width: '760px',
          maxWidth: '96vw',
          maxHeight: '96vh',
          data: { value: this.value() },
        },
      )
      .afterClosed()
      .subscribe((value) => {
        if (value === undefined) return;
        this.value.set(value);
        this.onChange(value);
        this.onTouched();
      });
  }

  iconName(value: string): string {
    return resourceIconName(value, 'category');
  }

  iconIsFill(value: string): boolean {
    return resourceIconStyle(value) === 'fill';
  }
}
