import { ChangeDetectionStrategy, Component, computed, forwardRef, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';

export const RESOURCE_COLOR_OPTIONS = [
  '#f44336',
  '#e91e63',
  '#9c27b0',
  '#673ab7',
  '#3f51b5',
  '#2196f3',
  '#03a9f4',
  '#00bcd4',
  '#009688',
  '#4caf50',
  '#8bc34a',
  '#cddc39',
  '#ffeb3b',
  '#ffc107',
  '#ff9800',
  '#ff5722',
  '#795548',
  '#607d8b',
  '#9e9e9e',
  '#263238',
] as const;

@Component({
  selector: 'app-resource-color-picker',
  imports: [MatButtonModule, MatIconModule, MatTooltipModule, TranslateModule],
  templateUrl: './color-picker.html',
  styleUrl: './color-picker.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ResourceColorPickerComponent),
      multi: true,
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceColorPickerComponent implements ControlValueAccessor {
  readonly i18nKeys = I18N_KEYS;
  readonly colors = RESOURCE_COLOR_OPTIONS;
  readonly value = signal('');
  readonly disabled = signal(false);
  readonly isCustomColor = computed(
    () => !!this.value() && !this.colors.includes(this.value() as (typeof RESOURCE_COLOR_OPTIONS)[number]),
  );
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

  select(color: string): void {
    if (this.disabled()) return;
    this.value.set(color);
    this.onChange(color);
    this.onTouched();
  }

  customColorChanged(event: Event): void {
    const color = (event.target as HTMLInputElement).value;
    this.select(color);
  }
}
