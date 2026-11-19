import {
  AuditMode,
  AuditRecordStatus,
  AuditStepStatus,
  AuditTaskStatus,
} from './audit-enums.model';

export interface AuditRecordTaskDto {
  id: string;
  auditRecordDetailId: string;
  assigneeUserId: string;
  assigneeNameSnapshot: string;
  status: AuditTaskStatus;
  comment?: string | null;
  actedTime?: string | null;
}

export interface AuditRecordStepDto {
  id: string;
  stepNoSnapshot: number;
  stepNameSnapshot: string;
  auditModeSnapshot: AuditMode;
  status: AuditStepStatus;
  startedTime?: string | null;
  completedTime?: string | null;
  tasks: AuditRecordTaskDto[];
}

export interface AuditRecordItemDto {
  id: string;
  flowNameSnapshot: string;
  sceneCode: string;
  bizId: string;
  applicantUserId: string;
  applicantNameSnapshot: string;
  status: AuditRecordStatus;
  currentStepNo?: number | null;
  submittedTime: string;
  completedTime?: string | null;
  withdrawnTime?: string | null;
}

export interface AuditRecordDetailDto extends AuditRecordItemDto {
  applyReason: string;
  formSnapshotJson?: string | null;
  withdrawReason?: string | null;
  steps: AuditRecordStepDto[];
}
