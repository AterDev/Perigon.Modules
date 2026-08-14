import { ResValueType } from '../entity/res-value-type.model';

export interface ResDefinitionPropertyDto {
  /** id */
  id?: string | null;
  /** name */
  name: string;
  /** 资源属性值类型。 */
  valueType: ResValueType;
  /** isRequired */
  isRequired: boolean;
  /** maxLength */
  maxLength: number;
  /** sort */
  sort: number;
}
