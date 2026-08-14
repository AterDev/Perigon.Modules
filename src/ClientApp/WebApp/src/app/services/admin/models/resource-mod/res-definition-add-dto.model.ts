import { ResDefinitionPropertyDto } from '../resource-mod/res-definition-property-dto.model';

export interface ResDefinitionAddDto {
  /** name */
  name: string;
  /** icon */
  icon?: string | null;
  /** properties */
  properties: ResDefinitionPropertyDto[];
}
