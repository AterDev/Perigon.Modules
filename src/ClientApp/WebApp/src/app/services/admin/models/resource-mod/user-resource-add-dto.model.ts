import { UserResourceStatus } from '../entity/user-resource-status.model';
import { ResourceValueDto } from '../resource-mod/resource-value-dto.model';

/**
 * 用户资源新增结构。
 */
export interface UserResourceAddDto {
  /** 资源定义 ID。 */
  definitionId: string;
  /** 用户资源的可见性。 */
  status: UserResourceStatus;
  /** 按资源定义填写的属性值。 */
  values: ResourceValueDto[];
}
