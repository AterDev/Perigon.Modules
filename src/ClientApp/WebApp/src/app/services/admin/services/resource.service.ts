import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { ResourceItemDto } from '../models/resource-mod/resource-item-dto.model';
import { ResourceDetailDto } from '../models/resource-mod/resource-detail-dto.model';
import { ResourceInput } from '../models/resource-mod/resource-input.model';
import { Resource } from '../models/entity/resource.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class ResourceService extends BaseService {
  /**
   * list
   * @param name string
   * @param environmentId string
   * @param categoryId string
   * @param groupId string
   * @param definitionId string
   * @param tagName string
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(name: string | null, environmentId: string | null, categoryId: string | null, groupId: string | null, definitionId: string | null, tagName: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<ResourceItemDto>> {
    const _url = `/api/Resource/list?name=${name ?? ''}&environmentId=${environmentId ?? ''}&categoryId=${categoryId ?? ''}&groupId=${groupId ?? ''}&definitionId=${definitionId ?? ''}&tagName=${tagName ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<ResourceItemDto>>('get', _url);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ResourceDetailDto> {
    const _url = `/api/Resource/${id}`;
    return this.request<ResourceDetailDto>('get', _url);
  }
  /**
   * update
   * @param id string
   * @param data ResourceInput
   */
  update(id: string, data: ResourceInput): Observable<boolean> {
    const _url = `/api/Resource/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/Resource/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * add
   * @param data ResourceInput
   */
  add(data: ResourceInput): Observable<Resource> {
    const _url = `/api/Resource`;
    return this.request<Resource>('post', _url, data);
  }
}