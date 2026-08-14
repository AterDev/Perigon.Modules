import { ResDefinitionProperty } from 'src/app/services/admin/models/entity/res-definition-property.model';
import { Resource } from 'src/app/services/admin/models/entity/resource.model';

/**
 * 资源属性定义配置。
 */
export interface ResDefinition {
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
  /** 资源定义名称。 */
  name: string;
  /** Material Icons 图标名称，以字符串形式持久化。 */
  icon?: string | null;
  /** 定义包含的属性。由管理器按关联排序后填充，用于 API 响应。 */
  properties: ResDefinitionProperty[];
  /** 使用此定义的资源。 */
  resources: Resource[];
}
