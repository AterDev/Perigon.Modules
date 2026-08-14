import { ResValueType } from '../entity/res-value-type.model';

/**
 * 资源定义中的单个属性配置。
 */
export interface ResDefinitionProperty {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** 属性名称。 */
  name: string;
  /** 资源属性值类型。 */
  valueType: ResValueType;
  /** 是否必填。 */
  isRequired: boolean;
  /** 属性值最大长度。 */
  maxLength: number;
  /** 显示排序。排序属于定义与属性的关联，属性本身不持久化该值。 */
  sort: number;
}
