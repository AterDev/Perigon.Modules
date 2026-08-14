/**
 * 资源标签配置。
 */
export interface ResTag {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
  /** 标签名称。 */
  name: string;
  /** 显示颜色，例如 CSS 十六进制颜色值。 */
  color: string;
  /** Material Icons 图标名称，以字符串形式持久化。 */
  icon?: string | null;
}
