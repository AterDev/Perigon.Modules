import { ResValueType } from '../entity/res-value-type.model';

export interface ResourceValueDetailDto {
  /** definitionPropertyId */
  definitionPropertyId: string;
  /** name */
  name: string;
  /** valueType */
  valueType: ResValueType;
  /** value */
  value: string;
}
