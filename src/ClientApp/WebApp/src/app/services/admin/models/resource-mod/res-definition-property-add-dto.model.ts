import { ResValueType } from '../entity/res-value-type.model';

/**
 * 资源属性定义新增请求结构。
 */
export interface ResDefinitionPropertyAddDto {
  /** 属性名称。 */
  name: string;
  /** 资源属性值类型。 */
  valueType: ResValueType;
  /** 是否为必填属性。 */
  isRequired: boolean;
  /** 属性值最大长度，取值范围为 1 到 1000。 */
  maxLength: number;
}
