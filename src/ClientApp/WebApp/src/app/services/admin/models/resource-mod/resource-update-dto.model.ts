import { ResourceValueDto } from '../resource-mod/resource-value-dto.model';

/**
 * 资源更新请求结构。
 */
export interface ResourceUpdateDto {
  /** 资源所属环境 ID。 */
  environmentId?: string | null;
  /** 资源所属分类 ID。 */
  categoryId?: string | null;
  /** 资源所属分组 ID，可选；分组必须属于所选分类。 */
  groupId?: string | null;
  /** 资源使用的定义 ID。 */
  definitionId?: string | null;
  /** 资源标签名称列表，允许为空。 */
  tagNames?: string[] | null;
  /** 按当前资源定义填写的属性值列表。 */
  values?: ResourceValueDto[] | null;
}
