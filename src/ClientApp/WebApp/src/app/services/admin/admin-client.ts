import { inject, Injectable } from '@angular/core';
import { ArticleService } from 'src/app/services/admin/services/article.service';
import { ArticleCategoryService } from 'src/app/services/admin/services/article-category.service';
import { ResourceService } from 'src/app/services/admin/services/resource.service';
import { ResourceConfigurationService } from 'src/app/services/admin/services/resource-configuration.service';
import { SystemConfigService } from 'src/app/services/admin/services/system-config.service';
import { SystemLogsService } from 'src/app/services/admin/services/system-logs.service';
import { SystemMenuService } from 'src/app/services/admin/services/system-menu.service';
import { SystemPermissionService } from 'src/app/services/admin/services/system-permission.service';
import { SystemPermissionGroupService } from 'src/app/services/admin/services/system-permission-group.service';
import { SystemRoleService } from 'src/app/services/admin/services/system-role.service';
import { SystemUserService } from 'src/app/services/admin/services/system-user.service';
@Injectable({
  providedIn: 'root'
})
export class AdminClient {
  /** Article */
  public article = inject(ArticleService);
  /** ArticleCategory */
  public articleCategory = inject(ArticleCategoryService);
  /** Resource */
  public resource = inject(ResourceService);
  /** ResourceConfiguration */
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
}
