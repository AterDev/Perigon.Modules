import { SysUser } from '../entity/sys-user.model';
import { SysRole } from '../entity/sys-role.model';

/**
 * 系统用户角色关联表
 */
export interface SysUserRole {
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
  /** 用户ID */
  userId: string;
  /** 角色ID */
  roleId: string;
  /** 系统用户 */
  user: SysUser;
  /** 系统角色 */
  role: SysRole;
}
