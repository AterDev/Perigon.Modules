import { ResDefinitionPropertyDto } from '../resource-mod/res-definition-property-dto.model';

export interface ResDefinitionUpdateDto {
  /** name */
  name?: string | null;
  /** icon */
  icon?: string | null;
  /** properties */
  properties?: ResDefinitionPropertyDto[] | null;
}
