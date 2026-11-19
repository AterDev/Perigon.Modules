import { AuditMode } from './audit-enums.model';

export interface AuditFlowStepDto {
  stepNo: number;
  name: string;
  auditorUserIds: string[];
  auditMode: AuditMode;
}

export interface AuditFlowItemDto {
  id: string;
  flowKey: string;
  sceneCode: string;
  name: string;
  isEnabled: boolean;
  createdTime: string;
  updatedTime: string;
}

export interface AuditFlowDetailDto extends AuditFlowItemDto {
  steps: AuditFlowStepDto[];
}

export interface AuditFlowWriteDto {
  flowKey: string;
  sceneCode: string;
  name: string;
  isEnabled: boolean;
  steps: AuditFlowStepDto[];
}
