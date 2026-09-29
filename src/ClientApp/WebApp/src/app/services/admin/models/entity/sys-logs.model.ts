import { UserActionType } from '../perigon/user-action-type.model';
import { SysUser } from '../entity/sys-user.model';

/**
 * 系统日志
 */
export interface SysLogs {
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
  /** 操作人名称 */
  actionUserName: string;
  /** 操作对象名称 */
  targetName?: string | null;
  /** 操作路由 */
  route: string;
  /** actionType */
  actionType: UserActionType;
  /** 描述 */
  description?: string | null;
  /** 系统用户 */
  sysUser: SysUser;
  /** sysUserId */
  sysUserId: string;
}
