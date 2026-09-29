import { SysDataScopeItemDto } from '../system-mod/sys-data-scope-item-dto.model';

/**
 * 当前用户的数据权限组及其范围。
 */
export interface SysUserDataScopeGroupItemDto {
  /** id */
  id: string;
  /** name */
  name: string;
  /** dataScopes */
  dataScopes: SysDataScopeItemDto[];
}
