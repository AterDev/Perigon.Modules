import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';
import { Resource } from 'src/app/services/admin/models/entity/resource.model';

/**
 * 资源分组配置。
 */
export interface ResGroup {
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
  /** 分组名称。 */
  name: string;
  /** 分组描述。 */
  description?: string | null;
  /** Material Icons 图标名称，以字符串形式持久化。 */
  icon?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
  /** 所属分类 ID。 */
  categoryId: string;
  /** 资源分类配置。 */
  category: ResCategory;
  /** 属于此分组的资源。 */
  resources: Resource[];
}
