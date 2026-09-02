/**
 * 资源分组新增请求结构。
 */
export interface ResGroupAddDto {
  /** 分组名称。 */
  name: string;
  /** 分组描述，可选。 */
  description?: string | null;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
  /** 所属资源分类 ID。 */
  categoryId: string;
}
