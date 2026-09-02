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
 * 资源管理。
 */
@Injectable({ providedIn: 'root' })
export class ResourceService extends BaseService {
  /**
   * 分页查询资源列表。
   * @param environmentId 按环境 ID 筛选。
   * @param categoryId 按分类 ID 筛选。
   * @param groupId 按分组 ID 筛选。
   * @param definitionId 按资源定义 ID 筛选。
   * @param tagName 按标签名称筛选。
   * @param searchKey 在资源定义名称、标签名称和属性值中进行关键字搜索；少于两个字符时不执行关键字搜索。
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(environmentId: string | null, categoryId: string | null, groupId: string | null, definitionId: string | null, tagName: string | null, searchKey: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<ResourceItemDto>> {
    const _url = `/api/Resource/list?environmentId=${environmentId ?? ''}&categoryId=${categoryId ?? ''}&groupId=${groupId ?? ''}&definitionId=${definitionId ?? ''}&tagName=${tagName ?? ''}&searchKey=${searchKey ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<ResourceItemDto>>('get', _url);
  }
  /**
   * 获取资源详情。
   * @param id 资源唯一标识。
   */
  detail(id: string): Observable<ResourceDetailDto> {
    const _url = `/api/Resource/${id}`;
    return this.request<ResourceDetailDto>('get', _url);
  }
  /**
   * 更新资源。
   * @param id 资源唯一标识。
   * @param data ResourceUpdateDto
   */
  update(id: string, data: ResourceUpdateDto): Observable<boolean> {
    const _url = `/api/Resource/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 删除资源。
   * @param id 资源唯一标识。
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/Resource/${id}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 新增资源。
   * @param data ResourceAddDto
   */
  add(data: ResourceAddDto): Observable<ResourceCreatedDto> {
    const _url = `/api/Resource`;
    return this.request<ResourceCreatedDto>('post', _url, data);
  }
}