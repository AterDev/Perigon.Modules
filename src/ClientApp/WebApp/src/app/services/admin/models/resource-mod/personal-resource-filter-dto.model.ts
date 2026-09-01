import { PersonalResourceAuditStatus } from './personal-resource-audit-status.model';
import { PersonalResourceStatus } from './personal-resource-status.model';

export interface PersonalResourceFilterDto {
  /** status */
  status?: PersonalResourceStatus | null;
  /** auditStatus */
  auditStatus?: PersonalResourceAuditStatus | null;
  /** pageIndex */
  pageIndex?: number | null;
  /** pageSize */
  pageSize?: number | null;
}
