import { SysMenu } from '../entity/sys-menu.model';
import { SysUserDataScopeGroupItemDto } from '../system-mod/sys-user-data-scope-group-item-dto.model';

export interface UserInfoDto {
  /** id */
  id: string;
  /** username */
  username: string;
  /** roles */
  roles: string[];
  /** menus */
  menus?: SysMenu[] | null;
  /** dataScopeGroups */
  dataScopeGroups: SysUserDataScopeGroupItemDto[];
}
