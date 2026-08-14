import { ResValueType } from 'src/app/services/admin/models/entity/res-value-type.model';
import { Resource } from 'src/app/services/admin/models/entity/resource.model';
import { ResDefinitionProperty } from 'src/app/services/admin/models/entity/res-definition-property.model';

/**
 * 资源实例的属性值。
 */
export interface ResValue {
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
  /** 资源 ID。 */
  resourceId: string;
  /** 资源定义属性 ID。 */
  definitionPropertyId: string;
  /** 以字符串形式持久化的属性值。 */
  value: string;
  /** 保存时的属性名称快照。 */
  propertyNameSnapshot: string;
  /** 资源属性值类型。 */
  valueTypeSnapshot: ResValueType;
  /** 按环境、分类和定义组织的资源实例。 */
  resource: Resource;
  /** 资源定义中的单个属性配置。 */
  definitionProperty: ResDefinitionProperty;
}
