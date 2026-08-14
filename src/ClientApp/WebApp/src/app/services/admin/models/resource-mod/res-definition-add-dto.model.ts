import { ResDefinitionPropertyDto } from 'src/app/services/admin/models/resource-mod/res-definition-property-dto.model';

export interface ResDefinitionAddDto {
  /** name */
  name: string;
  /** icon */
  icon?: string | null;
  /** properties */
  properties: ResDefinitionPropertyDto[];
}
