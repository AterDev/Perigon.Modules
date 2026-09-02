/**
 * 资源标签更新请求结构。
 */
export interface ResTagUpdateDto {
  /** 标签名称。 */
  name?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color?: string | null;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
}
