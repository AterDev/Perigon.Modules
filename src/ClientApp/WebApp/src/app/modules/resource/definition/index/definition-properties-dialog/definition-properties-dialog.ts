import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { ResDefinition } from 'src/app/services/admin/models/entity/res-definition.model';
import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';
import { sortResourceProperties } from 'src/app/modules/resource/shared/resource-property-order';

export interface ResourceDefinitionPropertiesDialogData {
  definition: ResDefinition;
}

@Component({
  selector: 'app-resource-definition-properties-dialog',
  imports: CommonFormModules,
  templateUrl: './definition-properties-dialog.html',
  styleUrl: './definition-properties-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceDefinitionPropertiesDialogComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly data = inject<ResourceDefinitionPropertiesDialogData>(MAT_DIALOG_DATA);
  readonly properties = sortResourceProperties(this.data.definition.properties);
  readonly valueTypeLabels = {
    [ResValueType.String]: I18N_KEYS.resource.propertyTypes.string,
    [ResValueType.Number]: I18N_KEYS.resource.propertyTypes.number,
    [ResValueType.Boolean]: I18N_KEYS.resource.propertyTypes.boolean,
    [ResValueType.Date]: I18N_KEYS.resource.propertyTypes.date,
    [ResValueType.Uri]: I18N_KEYS.resource.propertyTypes.uri,
    [ResValueType.IPAddress]: I18N_KEYS.resource.propertyTypes.ipAddress,
  };
}
