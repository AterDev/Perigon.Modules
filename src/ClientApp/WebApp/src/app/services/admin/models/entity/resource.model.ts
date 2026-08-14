import { ResEnvironment } from 'src/app/services/admin/models/entity/res-environment.model';
import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';
import { ResGroup } from 'src/app/services/admin/models/entity/res-group.model';
import { ResDefinition } from 'src/app/services/admin/models/entity/res-definition.model';
import { ResValue } from 'src/app/services/admin/models/entity/res-value.model';

/**
 * 按环境、分类和定义组织的资源实例。
 */
export interface Resource {
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
  /** 环境 ID。 */
  environmentId: string;
  /** 分类 ID。 */
  categoryId: string;
  /** 可选的分组 ID。 */
  groupId?: string | null;
  /** 资源定义 ID。 */
  definitionId: string;
  /** 资源关联的标签名称列表。 */
  tagNames: string[];
  /** 资源运行环境配置。 */
  environment: ResEnvironment;
  /** 资源分类配置。 */
  category: ResCategory;
  /** 资源分组配置。 */
  group: ResGroup;
  /** 资源属性定义配置。 */
  definition: ResDefinition;
  /** 资源属性值。 */
  values: ResValue[];
}
