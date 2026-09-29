import { SysUser } from '../entity/sys-user.model';

/**
 * 组织结构
 */
export interface SysOrganization {
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
  /** 名称 */
  name: string;
  /** 子目录 */
  children: SysOrganization[];
  /** 组织结构 */
  parent: SysOrganization;
  /** parentId */
  parentId?: string | null;
  /** users */
  users: SysUser[];
}
