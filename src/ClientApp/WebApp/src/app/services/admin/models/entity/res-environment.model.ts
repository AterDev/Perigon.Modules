import { Resource } from '../entity/resource.model';
import { ResPermission } from '../entity/res-permission.model';

/**
 * 资源运行环境配置。
 */
export interface ResEnvironment {
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
  /** 环境名称。 */
  name: string;
  /** Material Icons 图标名称，以字符串形式持久化。 */
  icon?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
  /** 属于此环境的资源。 */
  resources: Resource[];
  /** 此环境下的角色授权。 */
  permissions: ResPermission[];
}
