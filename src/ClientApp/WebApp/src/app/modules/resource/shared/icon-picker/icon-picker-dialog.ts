import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { I18N_KEYS } from '../../../share/i18n-keys';
import {
  RESOURCE_ICON_OPTIONS,
  resourceIconName,
  resourceIconStyle,
  resourceIconValue,
  ResourceIconStyle,
} from '../resource-appearance';

export interface ResourceIconPickerDialogData {
  value?: string;
}

@Component({
  selector: 'app-resource-icon-picker-dialog',
  imports: [
    MatButtonModule,
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatTabsModule,
    MatTooltipModule,
    TranslateModule,
  ],
  templateUrl: './icon-picker-dialog.html',
  styleUrl: './icon-picker-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceIconPickerDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly icons = RESOURCE_ICON_OPTIONS;
  readonly query = signal('');
  readonly currentName: string;
  readonly currentStyle: ResourceIconStyle;
  readonly filteredIcons = computed(() => {
    const query = this.query().trim().toLowerCase();
    const currentName = this.currentName;
    const options = currentName && !this.icons.includes(currentName as never)
      ? [currentName, ...this.icons]
      : this.icons;
    return query
      ? options.filter((icon) => icon.toLowerCase().includes(query))
      : options;
  });
  private readonly data = inject<ResourceIconPickerDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ResourceIconPickerDialogComponent>);

  constructor() {
    this.currentName = resourceIconName(this.data.value);
    this.currentStyle = resourceIconStyle(this.data.value);
  }

  searchChanged(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  select(icon: string, style: ResourceIconStyle): void {
    this.dialogRef.close(resourceIconValue(icon, style));
  }

  clear(): void {
    this.dialogRef.close('');
  }

  isFill(style: string): boolean {
    return style === 'fill';
  }
}
