import { SysRole } from '../entity/sys-role.model';
import { SysUserRole } from '../entity/sys-user-role.model';
import { SysLogs } from '../entity/sys-logs.model';
import { SysOrganization } from '../entity/sys-organization.model';
import { SysUserDataScopeGroup } from '../entity/sys-user-data-scope-group.model';
import { Sex } from '../entity/sex.model';

/**
 * 系统用户
 */
export interface SysUser {
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
  /** 用户名 */
  userName?: string | null;
  /** 真实姓名 */
  realName?: string | null;
  /** email */
  email: string;
  /** emailConfirmed */
  emailConfirmed: boolean;
  /** phoneNumber */
  phoneNumber?: string | null;
  /** phoneNumberConfirmed */
  phoneNumberConfirmed: boolean;
  /** twoFactorEnabled */
  twoFactorEnabled: boolean;
  /** lockoutEnd */
  lockoutEnd?: Date | null;
  /** lockoutEnabled */
  lockoutEnabled: boolean;
  /** accessFailedCount */
  accessFailedCount: number;
  /** 最后登录时间 */
  lastLoginTime?: Date | null;
  /** 最后密码修改时间 */
  lastPwdEditTime: Date;
  /** 密码重试次数 */
  retryCount: number;
  /** 头像url */
  avatar?: string | null;
  /** sysRoles */
  sysRoles: SysRole[];
  /** sysUserRoles */
  sysUserRoles: SysUserRole[];
  /** sysLogs */
  sysLogs: SysLogs[];
  /** sysOrganizations */
  sysOrganizations: SysOrganization[];
  /** 数据权限关联表 */
  dataScopeGroups: SysUserDataScopeGroup[];
  /** 性别 */
  sex: Sex;
}
