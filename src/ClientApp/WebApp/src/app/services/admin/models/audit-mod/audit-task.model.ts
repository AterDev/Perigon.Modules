import { AuditRecordDetailDto } from './audit-record.model';
import { AuditTaskStatus } from './audit-enums.model';

export interface AuditTaskItemDto {
  id: string;
  auditRecordId: string;
  auditRecordDetailId: string;
  assigneeUserId: string;
  assigneeNameSnapshot: string;
  status: AuditTaskStatus;
  stepNo: number;
  stepName: string;
  bizId: string;
  comment?: string | null;
  actedTime?: string | null;
}

export interface AuditTaskDetailDto extends AuditTaskItemDto {
  record: AuditRecordDetailDto;
}

export interface AuditDecisionDto {
  comment?: string | null;
}
