import { UserResourceStatus } from '../entity/user-resource-status.model';
import { UserResourceAuditStatus } from '../entity/user-resource-audit-status.model';

/**
 * 用户资源列表项结构。
 */
export interface UserResourceItemDto {
  /** 用户资源唯一标识。 */
  id: string;
  /** 资源所有者用户 ID。 */
  userId: string;
  /** 资源定义 ID。 */
  definitionId: string;
  /** 资源定义名称。 */
  definitionName: string;
  /** 用户资源的可见性。 */
  status: UserResourceStatus;
  /** 用户资源公开申请的审核状态。 */
  auditStatus: UserResourceAuditStatus;
  /** 审核通过后创建的常规资源 ID。 */
  approvedResourceId?: string | null;
  /** 审核意见或驳回原因。 */
  reviewComment?: string | null;
  /** 最后更新时间。 */
  updatedTime: Date;
}
