import { UserResourceStatus } from '../entity/user-resource-status.model';
import { ResourceValueDto } from '../resource-mod/resource-value-dto.model';

/**
 * 用户资源更新结构。
 */
export interface UserResourceUpdateDto {
  /** 资源定义 ID。 */
  definitionId?: string | null;
  /** 用户资源的可见性。 */
  status?: UserResourceStatus | null;
  /** 按资源定义填写的属性值。 */
  values?: ResourceValueDto[] | null;
}
