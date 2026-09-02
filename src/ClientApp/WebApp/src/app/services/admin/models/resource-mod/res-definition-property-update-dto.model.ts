import { ResValueType } from '../entity/res-value-type.model';

/**
 * 资源属性定义更新请求结构。
 */
export interface ResDefinitionPropertyUpdateDto {
  /** 属性名称。 */
  name?: string | null;
  /** 资源属性值类型。 */
  valueType?: ResValueType | null;
  /** 是否为必填属性。 */
  isRequired?: boolean | null;
  /** 属性值最大长度，取值范围为 1 到 1000。 */
  maxLength?: number | null;
}
