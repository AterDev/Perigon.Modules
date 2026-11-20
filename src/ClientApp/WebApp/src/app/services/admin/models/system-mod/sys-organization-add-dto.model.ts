/**
 * 组织添加时请求结构。
 */
export interface SysOrganizationAddDto {
  /** 名称 */
  name: string;
  /** 父组织 */
  parentId?: string | null;
  /** 外部系统中的组织标识 */
  externalId?: string | null;
}
