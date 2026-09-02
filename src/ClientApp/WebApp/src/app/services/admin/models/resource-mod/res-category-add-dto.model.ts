/**
 * 资源分类新增请求结构。
 */
export interface ResCategoryAddDto {
  /** 分类名称。 */
  name: string;
  /** 分类编码，在当前租户内必须唯一。 */
  catalogCode: string;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
}
