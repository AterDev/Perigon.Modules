import { SysDataScopeItemDto } from '../system-mod/sys-data-scope-item-dto.model';

export interface SysDataScopeGroupDetailDto {
  /** id */
  id: string;
  /** name */
  name: string;
  /** description */
  description?: string | null;
  /** isEnabled */
  isEnabled: boolean;
  /** dataScopes */
  dataScopes: SysDataScopeItemDto[];
  /** userIds */
  userIds: string[];
}
