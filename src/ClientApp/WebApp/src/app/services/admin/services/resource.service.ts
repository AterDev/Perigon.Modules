import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { ResourceItemDto } from '../models/resource-mod/resource-item-dto.model';
import { ResourceDetailDto } from '../models/resource-mod/resource-detail-dto.model';
import { ResourceUpdateDto } from '../models/resource-mod/resource-update-dto.model';
import { ResourceAddDto } from '../models/resource-mod/resource-add-dto.model';
import { ResourceCreatedDto } from '../models/resource-mod/resource-created-dto.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class ResourceService extends BaseService {
  /**
   * list
   * @param environmentId string
   * @param categoryId string
   * @param groupId string
   * @param definitionId string
   * @param tagName string
   * @param searchKey string
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(environmentId: string | null, categoryId: string | null, groupId: string | null, definitionId: string | null, tagName: string | null, searchKey: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<ResourceItemDto>> {
    const _url = `/api/Resource/list?environmentId=${environmentId ?? ''}&categoryId=${categoryId ?? ''}&groupId=${groupId ?? ''}&definitionId=${definitionId ?? ''}&tagName=${tagName ?? ''}&searchKey=${encodeURIComponent(searchKey ?? '')}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
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
   * @param data ResourceUpdateDto
   */
  update(id: string, data: ResourceUpdateDto): Observable<boolean> {
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
   * @param data ResourceAddDto
   */
  add(data: ResourceAddDto): Observable<ResourceCreatedDto> {
    const _url = `/api/Resource`;
    return this.request<ResourceCreatedDto>('post', _url, data);
  }
}
