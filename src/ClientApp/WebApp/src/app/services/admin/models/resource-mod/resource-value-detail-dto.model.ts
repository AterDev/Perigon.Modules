import { ResValueType } from '../entity/res-value-type.model';

/**
 * 资源属性值详情响应结构。
 */
export interface ResourceValueDetailDto {
  /** 资源定义属性 ID。 */
  definitionPropertyId: string;
  /** 保存时的属性名称快照。 */
  name: string;
  /** 资源属性值类型。 */
  valueType: ResValueType;
  /** 属性值。 */
  value: string;
}
