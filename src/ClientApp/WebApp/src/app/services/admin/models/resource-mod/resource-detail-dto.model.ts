import { ResourceValueDetailDto } from '../resource-mod/resource-value-detail-dto.model';

/**
 * 资源详情响应结构，包含资源的动态属性值。
 */
export interface ResourceDetailDto {
  /** 资源唯一标识。 */
  id: string;
  /** 资源所属环境 ID。 */
  environmentId: string;
  /** 资源所属环境名称。 */
  environmentName: string;
  /** 资源所属分类 ID。 */
  categoryId: string;
  /** 资源所属分类名称。 */
  categoryName: string;
  /** 资源所属分组 ID，可为空。 */
  groupId?: string | null;
  /** 资源所属分组名称，可为空。 */
  groupName?: string | null;
  /** 资源使用的定义 ID。 */
  definitionId: string;
  /** 资源定义名称。 */
  definitionName: string;
  /** 资源标签名称列表。 */
  tagNames: string[];
  /** 最后更新时间。 */
  updatedTime: Date;
  /** 资源属性值列表，包含保存时的名称和类型快照。 */
  values: ResourceValueDetailDto[];
}
