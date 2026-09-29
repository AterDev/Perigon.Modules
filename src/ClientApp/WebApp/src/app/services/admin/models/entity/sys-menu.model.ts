import { SysRole } from '../entity/sys-role.model';
import { MenuType } from '../entity/menu-type.model';

/**
 * 系统菜单
 */
export interface SysMenu {
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
  /** 菜单名称 */
  name: string;
  /** 菜单路径 */
  path?: string | null;
  /** 图标 */
  icon?: string | null;
  /** 系统菜单 */
  parent: SysMenu;
  /** parentId */
  parentId?: string | null;
  /** 是否有效 */
  isValid: boolean;
  /** 子菜单 */
  children: SysMenu[];
  /** 所属角色 */
  sysRoles: SysRole[];
  /** 权限编码 */
  accessCode: string;
  /** menuType */
  menuType: MenuType;
  /** 排序 */
  sort: number;
  /** 是否显示 */
  hidden: boolean;
}
