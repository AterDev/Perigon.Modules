import { ResValueType } from '../entity/res-value-type.model';

export interface ResDefinitionPropertyInput {
  /** id */
  id?: string | null;
  /** name */
  name: string;
  /** valueType */
  valueType: ResValueType;
  /** isRequired */
  isRequired: boolean;
  /** maxLength */
  maxLength: number;
  /** sort */
  sort: number;
}
