/**
 * 资源权限替换请求结构。
 */
export interface ResPermissionUpdateDto {
  /** 资源环境 ID。 */
  environmentId?: string | null;
  /** 资源分类 ID。 */
  categoryId?: string | null;
  /** 允许查看该环境和分类资源的角色 ID 列表；提交时会去重并整体替换原授权。 */
  roleIds?: string[] | null;
}
