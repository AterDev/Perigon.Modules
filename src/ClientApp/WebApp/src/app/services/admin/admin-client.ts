import { inject, Injectable } from '@angular/core';
import { ArticleService } from './services/article.service';
import { ArticleCategoryService } from './services/article-category.service';
import { ResourceService } from './services/resource.service';
import { ResourceConfigurationService } from './services/resource-configuration.service';
import { SystemConfigService } from './services/system-config.service';
import { SystemLogsService } from './services/system-logs.service';
import { SystemMenuService } from './services/system-menu.service';
import { SystemPermissionService } from './services/system-permission.service';
import { SystemPermissionGroupService } from './services/system-permission-group.service';
import { SystemRoleService } from './services/system-role.service';
import { SystemUserService } from './services/system-user.service';
import { UserFavoriteResourceService } from './services/user-favorite-resource.service';
import { UserResourceService } from './services/user-resource.service';
@Injectable({
  providedIn: 'root'
})
export class AdminClient {
  /** 文章管理。 */
  public article = inject(ArticleService);
  /** ArticleCategory */
  public articleCategory = inject(ArticleCategoryService);
  /** 资源管理。 */
  public resource = inject(ResourceService);
  /** 资源基础配置管理，包括环境、分类、分组、标签、属性定义、资源定义和资源权限。 */
  public resourceConfiguration = inject(ResourceConfigurationService);
  /** 系统配置 */
  public systemConfig = inject(SystemConfigService);
  /** 系统日志 */
  public systemLogs = inject(SystemLogsService);
  /** 系统菜单 */
  public systemMenu = inject(SystemMenuService);
  /** 权限 */
  public systemPermission = inject(SystemPermissionService);
  /** SystemPermissionGroup */
  public systemPermissionGroup = inject(SystemPermissionGroupService);
  /** 系统角色
SystemMod.Managers.SystemRoleManager */
  public systemRole = inject(SystemRoleService);
  /** 系统用户 */
  public systemUser = inject(SystemUserService);
  /** 用户收藏资源接口。 */
  public userFavoriteResource = inject(UserFavoriteResourceService);
  /** 用户资源提交和公开申请审核。 */
  public userResource = inject(UserResourceService);
}
