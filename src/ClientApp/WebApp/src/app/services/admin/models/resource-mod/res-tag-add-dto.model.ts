/**
 * 资源标签新增请求结构。
 */
export interface ResTagAddDto {
  /** 标签名称。 */
  name: string;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
}
