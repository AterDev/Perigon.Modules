/**
 * 组织更新时请求结构。
 */
export interface SysOrganizationUpdateDto {
  /** 名称 */
  name: string;
  /** 父组织 */
  parentId?: string | null;
  /** 外部系统中的组织标识 */
  externalId?: string | null;
}
