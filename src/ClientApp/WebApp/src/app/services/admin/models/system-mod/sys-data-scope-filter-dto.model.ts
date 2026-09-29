import { DataScopeType } from '../entity/data-scope-type.model';

/**
 * 数据权限范围查询条件。
 */
export interface SysDataScopeFilterDto {
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
  /** orderBy */
  orderBy?: Record<string, boolean> | null;
  /** name */
  name?: string | null;
  /** resourceCode */
  resourceCode?: string | null;
  /** 权限范围类型 */
  scopeType?: DataScopeType | null;
  /** groupId */
  groupId?: string | null;
}
