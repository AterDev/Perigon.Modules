import { ResourceValueDto } from '../resource-mod/resource-value-dto.model';

/**
 * 资源新增请求结构。
 */
export interface ResourceAddDto {
  /** 资源所属环境 ID。 */
  environmentId: string;
  /** 资源所属分类 ID。 */
  categoryId: string;
  /** 资源所属分组 ID，可选；分组必须属于所选分类。 */
  groupId?: string | null;
  /** 资源使用的定义 ID。 */
  definitionId: string;
  /** 资源标签名称列表，允许为空。 */
  tagNames: string[];
  /** 按资源定义填写的属性值列表。 */
  values: ResourceValueDto[];
}
