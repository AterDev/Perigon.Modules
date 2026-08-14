import { ResGroup } from 'src/app/services/admin/models/entity/res-group.model';
import { Resource } from 'src/app/services/admin/models/entity/resource.model';
import { ResPermission } from 'src/app/services/admin/models/entity/res-permission.model';

/**
 * 资源分类配置。
 */
export interface ResCategory {
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
  /** 分类名称。 */
  name: string;
  /** 分类编码。 */
  catalogCode: string;
  /** Material Icons 图标名称，以字符串形式持久化。 */
  icon?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
  /** 属于此分类的分组。 */
  groups: ResGroup[];
  /** 属于此分类的资源。 */
  resources: Resource[];
  /** 此分类下的角色授权。 */
  permissions: ResPermission[];
}
