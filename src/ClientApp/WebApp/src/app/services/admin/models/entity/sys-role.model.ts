import { SysUser } from '../entity/sys-user.model';
import { SysUserRole } from '../entity/sys-user-role.model';
import { SysMenu } from '../entity/sys-menu.model';

/**
 * 系统角色
 */
export interface SysRole {
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
  /** 角色名称 */
  name: string;
  /** 角色标识 */
  nameValue: string;
  /** 是否系统内置 */
  isSystem: boolean;
  /** 图标 */
  icon?: string | null;
  /** users */
  users: SysUser[];
  /** sysUserRoles */
  sysUserRoles: SysUserRole[];
  /** 菜单权限 */
  sysMenus: SysMenu[];
}
