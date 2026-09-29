import { DataScopeType } from '../entity/data-scope-type.model';
import { SysDataScopeGroup } from '../entity/sys-data-scope-group.model';

/**
 * 权限
 */
export interface SysDataScope {
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
  /** 权限名称标识 */
  name: string;
  /** 数据标识Code */
  resourceCode: string;
  /** 允许访问的数据标识 */
  targetIds: string[];
  /** 权限范围类型 */
  scopeType: DataScopeType;
  /** 数据权限组 */
  group: SysDataScopeGroup;
  /** groupId */
  groupId: string;
}
