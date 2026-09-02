/**
 * 资源环境新增请求结构。
 */
export interface ResEnvironmentAddDto {
  /** 环境名称。 */
  name: string;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
}
