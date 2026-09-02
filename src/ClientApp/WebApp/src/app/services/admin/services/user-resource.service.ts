import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { UserResourceStatus } from '../models/entity/user-resource-status.model';
import { UserResourceAuditStatus } from '../models/entity/user-resource-audit-status.model';
import { UserResourceItemDto } from '../models/resource-mod/user-resource-item-dto.model';
import { UserResourceDetailDto } from '../models/resource-mod/user-resource-detail-dto.model';
import { UserResourceUpdateDto } from '../models/resource-mod/user-resource-update-dto.model';
import { UserResourceAddDto } from '../models/resource-mod/user-resource-add-dto.model';
import { UserResourceCreatedDto } from '../models/resource-mod/user-resource-created-dto.model';
import { UserResourceReviewDto } from '../models/resource-mod/user-resource-review-dto.model';
import { UserResourceRejectDto } from '../models/resource-mod/user-resource-reject-dto.model';
/**
 * 用户资源提交和公开申请审核。
 */
@Injectable({ providedIn: 'root' })
export class UserResourceService extends BaseService {
  /**
   * 查询当前登录用户的用户资源。
   * @param status 按用户资源状态筛选。
   * @param auditStatus 按审核状态筛选。
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  mine(status: UserResourceStatus | null, auditStatus: UserResourceAuditStatus | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<UserResourceItemDto>> {
    const _url = `/api/UserResource/mine?status=${status ?? ''}&auditStatus=${auditStatus ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<UserResourceItemDto>>('get', _url);
  }
  /**
   * 查询待审核的公开申请。
   * @param status 按用户资源状态筛选。
   * @param auditStatus 按审核状态筛选。
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  review(status: UserResourceStatus | null, auditStatus: UserResourceAuditStatus | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<UserResourceItemDto>> {
    const _url = `/api/UserResource/review?status=${status ?? ''}&auditStatus=${auditStatus ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<UserResourceItemDto>>('get', _url);
  }
  /**
   * 获取用户资源详情。
   * @param id string
   */
  detail(id: string): Observable<UserResourceDetailDto> {
    const _url = `/api/UserResource/${id}`;
    return this.request<UserResourceDetailDto>('get', _url);
  }
  /**
   * 更新用户资源或重新提交公开申请。
   * @param id string
   * @param data UserResourceUpdateDto
   */
  update(id: string, data: UserResourceUpdateDto): Observable<boolean> {
    const _url = `/api/UserResource/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 删除用户资源。
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/UserResource/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 新增用户资源或提交公开申请。
   * @param data UserResourceAddDto
   */
  add(data: UserResourceAddDto): Observable<UserResourceCreatedDto> {
    const _url = `/api/UserResource`;
    return this.request<UserResourceCreatedDto>('post', _url, data);
  }
  /**
   * 审核通过公开申请并创建常规资源。
   * @param id string
   * @param data UserResourceReviewDto
   */
  approve(id: string, data: UserResourceReviewDto): Observable<boolean> {
    const _url = `/api/UserResource/${id}/approve`;
    return this.request<boolean>('post', _url, data);
  }
  /**
   * 驳回公开申请。
   * @param id string
   * @param data UserResourceRejectDto
   */
  reject(id: string, data: UserResourceRejectDto): Observable<boolean> {
    const _url = `/api/UserResource/${id}/reject`;
    return this.request<boolean>('post', _url, data);
  }
}