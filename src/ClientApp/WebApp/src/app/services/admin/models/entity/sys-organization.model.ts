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
  /** 名称 */
  name: string;
  /** 外部系统中的组织标识 */
  externalId?: string | null;
  /** 子目录 */
  children: SysOrganization[];
  /** 组织结构 */
  parent?: SysOrganization | null;
  /** parentId */
  parentId?: string | null;
  /** users */
  users: SysUser[];
}
