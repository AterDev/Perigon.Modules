import { DataScopeType } from '../entity/data-scope-type.model';

/**
 * 数据权限范围详情。
 */
export interface SysDataScopeDetailDto {
  /** id */
  id: string;
  /** name */
  name: string;
  /** resourceCode */
  resourceCode: string;
  /** targetIds */
  targetIds: string[];
  /** 权限范围类型 */
  scopeType: DataScopeType;
  /** groupId */
  groupId: string;
}
