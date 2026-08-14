import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';

export interface ResDefinitionPropertyUpdateDto {
  /** name */
  name?: string | null;
  /** 资源属性值类型。 */
  valueType?: ResValueType | null;
  /** isRequired */
  isRequired?: boolean | null;
  /** maxLength */
  maxLength?: number | null;
}
