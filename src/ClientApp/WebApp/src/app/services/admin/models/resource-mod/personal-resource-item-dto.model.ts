import { PersonalResourceAuditStatus } from './personal-resource-audit-status.model';
import { PersonalResourceStatus } from './personal-resource-status.model';

export interface PersonalResourceItemDto {
  /** id */
  id: string;
  /** userId */
  userId: string;
  /** definitionId */
  definitionId: string;
  /** definitionName */
  definitionName: string;
  /** status */
  status: PersonalResourceStatus;
  /** auditStatus */
  auditStatus: PersonalResourceAuditStatus;
  /** approvedResourceId */
  approvedResourceId?: string | null;
  /** reviewComment */
  reviewComment?: string | null;
  /** updatedTime */
  updatedTime: Date;
}
