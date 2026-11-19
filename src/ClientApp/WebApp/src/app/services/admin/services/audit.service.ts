import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseService } from '../base.service';
import { PageList } from '../models/perigon/page-list.model';
import {
  AuditFlowDetailDto,
  AuditFlowItemDto,
  AuditFlowWriteDto,
} from '../models/audit-mod/audit-flow.model';
import {
  AuditRecordDetailDto,
  AuditRecordItemDto,
} from '../models/audit-mod/audit-record.model';
import {
  AuditDecisionDto,
  AuditTaskDetailDto,
  AuditTaskItemDto,
} from '../models/audit-mod/audit-task.model';
import {
  AuditRecordStatus,
  AuditTaskStatus,
} from '../models/audit-mod/audit-enums.model';

@Injectable({ providedIn: 'root' })
export class AuditService extends BaseService {
  flows(
    sceneCode?: string | null,
    keyword?: string | null,
    isEnabled?: boolean | null,
    pageIndex = 1,
    pageSize = 20,
  ): Observable<PageList<AuditFlowItemDto>> {
    const params = this.query({
      sceneCode,
      keyword,
      isEnabled,
      pageIndex,
      pageSize,
    });
    return this.request<PageList<AuditFlowItemDto>>(
      'get',
      `/api/admin/audit-flows${params}`,
    );
  }

  flow(id: string): Observable<AuditFlowDetailDto> {
    return this.request<AuditFlowDetailDto>(
      'get',
      `/api/admin/audit-flows/${id}`,
    );
  }

  addFlow(data: AuditFlowWriteDto): Observable<AuditFlowDetailDto> {
    return this.request<AuditFlowDetailDto>(
      'post',
      '/api/admin/audit-flows',
      data,
    );
  }

  updateFlow(
    id: string,
    data: AuditFlowWriteDto,
  ): Observable<AuditFlowDetailDto> {
    return this.request<AuditFlowDetailDto>(
      'put',
      `/api/admin/audit-flows/${id}`,
      data,
    );
  }

  setFlowEnabled(id: string, enabled: boolean): Observable<void> {
    return this.request<void>('patch', `/api/admin/audit-flows/${id}/enabled`, {
      enabled,
    });
  }

  deleteFlow(id: string): Observable<void> {
    return this.request<void>('delete', `/api/admin/audit-flows/${id}`);
  }

  records(
    status?: AuditRecordStatus | null,
    sceneCode?: string | null,
    bizId?: string | null,
    pageIndex = 1,
    pageSize = 20,
  ): Observable<PageList<AuditRecordItemDto>> {
    const params = this.query({
      status,
      sceneCode,
      bizId,
      pageIndex,
      pageSize,
    });
    return this.request<PageList<AuditRecordItemDto>>(
      'get',
      `/api/admin/audit-records${params}`,
    );
  }

  myRecords(
    status?: AuditRecordStatus | null,
    sceneCode?: string | null,
    bizId?: string | null,
    pageIndex = 1,
    pageSize = 20,
  ): Observable<PageList<AuditRecordItemDto>> {
    const params = this.query({
      status,
      sceneCode,
      bizId,
      pageIndex,
      pageSize,
    });
    return this.request<PageList<AuditRecordItemDto>>(
      'get',
      `/api/audit-records/mine${params}`,
    );
  }

  record(id: string): Observable<AuditRecordDetailDto> {
    return this.request<AuditRecordDetailDto>(
      'get',
      `/api/audit-records/${id}`,
    );
  }

  withdrawRecord(
    id: string,
    reason?: string | null,
  ): Observable<AuditRecordDetailDto> {
    return this.request<AuditRecordDetailDto>(
      'post',
      `/api/audit-records/${id}/withdraw`,
      { reason: reason || null },
    );
  }

  tasks(
    status?: AuditTaskStatus | null,
    pageIndex = 1,
    pageSize = 20,
  ): Observable<PageList<AuditTaskItemDto>> {
    const params = this.query({ status, pageIndex, pageSize });
    return this.request<PageList<AuditTaskItemDto>>(
      'get',
      `/api/audit-tasks/mine${params}`,
    );
  }

  task(id: string): Observable<AuditTaskDetailDto> {
    return this.request<AuditTaskDetailDto>('get', `/api/audit-tasks/${id}`);
  }

  approveTask(
    id: string,
    data: AuditDecisionDto,
  ): Observable<AuditTaskDetailDto> {
    return this.request<AuditTaskDetailDto>(
      'post',
      `/api/audit-tasks/${id}/approve`,
      data,
    );
  }

  rejectTask(
    id: string,
    data: AuditDecisionDto,
  ): Observable<AuditTaskDetailDto> {
    return this.request<AuditTaskDetailDto>(
      'post',
      `/api/audit-tasks/${id}/reject`,
      data,
    );
  }

  private query(values: Record<string, unknown>): string {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(values)) {
      if (value !== undefined && value !== null && value !== '') {
        params.set(key, String(value));
      }
    }
    const text = params.toString();
    return text ? `?${text}` : '';
  }
}
