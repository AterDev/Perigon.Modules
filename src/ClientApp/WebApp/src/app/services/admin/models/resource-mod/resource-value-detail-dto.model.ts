import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';

export interface ResourceValueDetailDto {
  /** definitionPropertyId */
  definitionPropertyId: string;
  /** name */
  name: string;
  /** 资源属性值类型。 */
  valueType: ResValueType;
  /** value */
  value: string;
}
