/**
 * 资源分类更新请求结构。
 */
export interface ResCategoryUpdateDto {
  /** 分类名称。 */
  name?: string | null;
  /** 分类编码，在当前租户内必须唯一。 */
  catalogCode?: string | null;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color?: string | null;
}
