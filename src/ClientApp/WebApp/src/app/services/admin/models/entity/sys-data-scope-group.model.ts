import { SysDataScope } from '../entity/sys-data-scope.model';
import { SysUserDataScopeGroup } from '../entity/sys-user-data-scope-group.model';

/**
 * 数据权限组
 */
export interface SysDataScopeGroup {
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
  /** 分组名称 */
  name: string;
  /** 权限说明 */
  description?: string | null;
  /** 是否启用 */
  isEnabled: boolean;
  /** dataScopes */
  dataScopes: SysDataScope[];
  /** 数据权限关联表 */
  users: SysUserDataScopeGroup[];
}
