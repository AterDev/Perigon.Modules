import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { ResDefinitionProperty } from 'src/app/services/admin/models/entity/res-definition-property.model';
import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { sortResourceProperties } from 'src/app/modules/resource/shared/resource-property-order';

export interface ResourcePropertySelectDialogData {
  excludeIds?: string[];
}

@Component({
  selector: 'app-resource-property-select-dialog',
  imports: CommonFormModules,
  templateUrl: './property-select-dialog.html',
  styleUrl: './property-select-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourcePropertySelectDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly properties = signal<ResDefinitionProperty[]>([]);
  readonly selectedIds = new Set<string>();
  readonly valueTypeLabels = {
    [ResValueType.String]: I18N_KEYS.resource.propertyTypes.string,
    [ResValueType.Number]: I18N_KEYS.resource.propertyTypes.number,
    [ResValueType.Boolean]: I18N_KEYS.resource.propertyTypes.boolean,
    [ResValueType.Date]: I18N_KEYS.resource.propertyTypes.date,
    [ResValueType.Uri]: I18N_KEYS.resource.propertyTypes.uri,
    [ResValueType.IPAddress]: I18N_KEYS.resource.propertyTypes.ipAddress,
  };
  readonly data = inject<ResourcePropertySelectDialogData>(MAT_DIALOG_DATA, { optional: true });
  private readonly client = inject(AdminClient);
  private readonly dialogRef = inject(MatDialogRef<ResourcePropertySelectDialogComponent>);

  constructor() {
    const excluded = new Set(this.data?.excludeIds ?? []);
    this.client.resourceConfiguration.properties(null).subscribe((properties) => {
      this.properties.set(sortResourceProperties(
        properties.filter((property) => !excluded.has(property.id)),
      ));
    });
  }

  toggle(propertyId: string, change: MatCheckboxChange): void {
    if (change.checked) {
      this.selectedIds.add(propertyId);
    } else {
      this.selectedIds.delete(propertyId);
    }
  }

  save(): void {
    this.dialogRef.close(this.properties().filter((property) => this.selectedIds.has(property.id)));
  }
}
