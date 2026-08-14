import { ResEnvironment } from 'src/app/services/admin/models/entity/res-environment.model';
import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';

/**
 * 资源环境和分类的角色读取授权。
 */
export interface ResPermission {
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
  /** 角色 ID。 */
  roleId: string;
  /** 环境 ID。 */
  environmentId: string;
  /** 分类 ID。 */
  categoryId: string;
  /** 资源运行环境配置。 */
  environment: ResEnvironment;
  /** 资源分类配置。 */
  category: ResCategory;
}
