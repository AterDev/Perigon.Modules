import { ResValueType } from '../entity/res-value-type.model';

/**
 * 资源定义中的属性配置请求结构。
 */
export interface ResDefinitionPropertyDto {
  /** 已有资源属性的唯一标识；为空时按名称匹配已有属性或创建新属性。 */
  id?: string | null;
  /** 属性名称。 */
  name: string;
  /** 资源属性值类型。 */
  valueType: ResValueType;
  /** 是否为必填属性。 */
  isRequired: boolean;
  /** 属性值最大长度，取值范围为 1 到 1000。 */
  maxLength: number;
  /** 属性在资源定义中的显示排序。 */
  sort: number;
}
