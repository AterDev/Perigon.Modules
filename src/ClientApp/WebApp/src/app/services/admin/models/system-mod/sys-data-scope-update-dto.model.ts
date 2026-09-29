import { DataScopeType } from '../entity/data-scope-type.model';

/**
 * 更新数据权限范围。
 */
export interface SysDataScopeUpdateDto {
  /** name */
  name?: string | null;
  /** resourceCode */
  resourceCode?: string | null;
  /** targetIds */
  targetIds?: string[] | null;
  /** 权限范围类型 */
  scopeType?: DataScopeType | null;
  /** groupId */
  groupId?: string | null;
}
