import { ResDefinitionPropertyInput } from '../resource-mod/res-definition-property-input.model';

export interface ResDefinitionInput {
  /** name */
  name: string;
  /** icon */
  icon?: string | null;
  /** properties */
  properties: ResDefinitionPropertyInput[];
}
