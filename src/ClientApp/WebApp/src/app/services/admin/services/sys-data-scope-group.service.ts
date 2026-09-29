import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SysDataScopeGroupFilterDto } from '../models/system-mod/sys-data-scope-group-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SysDataScopeGroupItemDto } from '../models/system-mod/sys-data-scope-group-item-dto.model';
import { SysDataScopeGroupAddDto } from '../models/system-mod/sys-data-scope-group-add-dto.model';
import { SysDataScopeGroup } from '../models/entity/sys-data-scope-group.model';
import { SysDataScopeGroupUpdateDto } from '../models/system-mod/sys-data-scope-group-update-dto.model';
import { SysDataScopeGroupDetailDto } from '../models/system-mod/sys-data-scope-group-detail-dto.model';
import { SysUserDataScopeGroupSetUsersDto } from '../models/system-mod/sys-user-data-scope-group-set-users-dto.model';
/**
 * 数据权限组管理接口。
 */
@Injectable({ providedIn: 'root' })
export class SysDataScopeGroupService extends BaseService {
  /**
   * filter
   * @param data SysDataScopeGroupFilterDto
   */
  filter(data: SysDataScopeGroupFilterDto): Observable<PageList<SysDataScopeGroupItemDto>> {
    const _url = `/api/SysDataScopeGroup/filter`;
    return this.request<PageList<SysDataScopeGroupItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data SysDataScopeGroupAddDto
   */
  add(data: SysDataScopeGroupAddDto): Observable<SysDataScopeGroup> {
    const _url = `/api/SysDataScopeGroup`;
    return this.request<SysDataScopeGroup>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data SysDataScopeGroupUpdateDto
   */
  update(id: string, data: SysDataScopeGroupUpdateDto): Observable<SysDataScopeGroup> {
    const _url = `/api/SysDataScopeGroup/${id}`;
    return this.request<SysDataScopeGroup>('patch', _url, data);
  }
  /**
   * getDetail
   * @param id string
   */
  getDetail(id: string): Observable<SysDataScopeGroupDetailDto> {
    const _url = `/api/SysDataScopeGroup/${id}`;
    return this.request<SysDataScopeGroupDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<void> {
    const _url = `/api/SysDataScopeGroup/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * getUserIds
   * @param id string
   */
  getUserIds(id: string): Observable<string[]> {
    const _url = `/api/SysDataScopeGroup/${id}/users`;
    return this.request<string[]>('get', _url);
  }
  /**
   * setUsers
   * @param id string
   * @param data SysUserDataScopeGroupSetUsersDto
   */
  setUsers(id: string, data: SysUserDataScopeGroupSetUsersDto): Observable<void> {
    const _url = `/api/SysDataScopeGroup/${id}/users`;
    return this.request<void>('put', _url, data);
  }
}