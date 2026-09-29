import { SysUser } from '../entity/sys-user.model';
import { SysDataScopeGroup } from '../entity/sys-data-scope-group.model';

/**
 * 用户数据权限中间表
 */
export interface SysUserDataScopeGroup {
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
  /** userId */
  userId: string;
  /** dataScopeGroupId */
  dataScopeGroupId: string;
  /** 系统用户 */
  user: SysUser;
  /** 数据权限组 */
  dataScopeGroup: SysDataScopeGroup;
}
