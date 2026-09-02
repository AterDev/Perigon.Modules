import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseService } from '../base.service';
import { PageList } from '../models/perigon/page-list.model';
import { PersonalResourceAddDto } from '../models/resource-mod/personal-resource-add-dto.model';
import { PersonalResourceDetailDto } from '../models/resource-mod/personal-resource-detail-dto.model';
import { PersonalResourceFilterDto } from '../models/resource-mod/personal-resource-filter-dto.model';
import { PersonalResourceItemDto } from '../models/resource-mod/personal-resource-item-dto.model';
import { PersonalResourceRejectDto } from '../models/resource-mod/personal-resource-reject-dto.model';
import { PersonalResourceReviewDto } from '../models/resource-mod/personal-resource-review-dto.model';
import { PersonalResourceUpdateDto } from '../models/resource-mod/personal-resource-update-dto.model';

@Injectable({ providedIn: 'root' })
export class PersonalResourceService extends BaseService {
  mine(filter: PersonalResourceFilterDto = {}): Observable<PageList<PersonalResourceItemDto>> {
    return this.request<PageList<PersonalResourceItemDto>>('get', this.listUrl('mine', filter));
  }

  review(filter: PersonalResourceFilterDto = {}): Observable<PageList<PersonalResourceItemDto>> {
    return this.request<PageList<PersonalResourceItemDto>>('get', this.listUrl('review', filter));
  }

  detail(id: string): Observable<PersonalResourceDetailDto> {
    return this.request<PersonalResourceDetailDto>('get', `/api/PersonalResource/${id}`);
  }

  add(data: PersonalResourceAddDto): Observable<{ id: string }> {
    return this.request<{ id: string }>('post', '/api/PersonalResource', data);
  }

  update(id: string, data: PersonalResourceUpdateDto): Observable<boolean> {
    return this.request<boolean>('patch', `/api/PersonalResource/${id}`, data);
  }

  delete(id: string): Observable<boolean> {
    return this.request<boolean>('delete', `/api/PersonalResource/${id}`);
  }

  approve(id: string, data: PersonalResourceReviewDto): Observable<boolean> {
    return this.request<boolean>('post', `/api/PersonalResource/${id}/approve`, data);
  }

  reject(id: string, data: PersonalResourceRejectDto): Observable<boolean> {
    return this.request<boolean>('post', `/api/PersonalResource/${id}/reject`, data);
  }

  private listUrl(path: string, filter: PersonalResourceFilterDto): string {
    const params = new URLSearchParams();
    if (filter.status !== undefined && filter.status !== null) {
      params.set('status', String(filter.status));
    }
    if (filter.auditStatus !== undefined && filter.auditStatus !== null) {
      params.set('auditStatus', String(filter.auditStatus));
    }
    if (filter.pageIndex !== undefined && filter.pageIndex !== null) {
      params.set('pageIndex', String(filter.pageIndex));
    }
    if (filter.pageSize !== undefined && filter.pageSize !== null) {
      params.set('pageSize', String(filter.pageSize));
    }

    const query = params.toString();
    return query.length > 0 ? `/api/PersonalResource/${path}?${query}` : `/api/PersonalResource/${path}`;
  }
}
