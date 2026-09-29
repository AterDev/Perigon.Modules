import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SysDataScopeFilterDto } from '../models/system-mod/sys-data-scope-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SysDataScopeItemDto } from '../models/system-mod/sys-data-scope-item-dto.model';
import { SysDataScopeAddDto } from '../models/system-mod/sys-data-scope-add-dto.model';
import { SysDataScope } from '../models/entity/sys-data-scope.model';
import { SysDataScopeUpdateDto } from '../models/system-mod/sys-data-scope-update-dto.model';
import { SysDataScopeDetailDto } from '../models/system-mod/sys-data-scope-detail-dto.model';
/**
 * 数据权限范围管理接口。
 */
@Injectable({ providedIn: 'root' })
export class SysDataScopeService extends BaseService {
  /**
   * filter
   * @param data SysDataScopeFilterDto
   */
  filter(data: SysDataScopeFilterDto): Observable<PageList<SysDataScopeItemDto>> {
    const _url = `/api/SysDataScope/filter`;
    return this.request<PageList<SysDataScopeItemDto>>('post', _url, data);
  }
  /**
   * add
   * @param data SysDataScopeAddDto
   */
  add(data: SysDataScopeAddDto): Observable<SysDataScope> {
    const _url = `/api/SysDataScope`;
    return this.request<SysDataScope>('post', _url, data);
  }
  /**
   * update
   * @param id string
   * @param data SysDataScopeUpdateDto
   */
  update(id: string, data: SysDataScopeUpdateDto): Observable<SysDataScope> {
    const _url = `/api/SysDataScope/${id}`;
    return this.request<SysDataScope>('patch', _url, data);
  }
  /**
   * getDetail
   * @param id string
   */
  getDetail(id: string): Observable<SysDataScopeDetailDto> {
    const _url = `/api/SysDataScope/${id}`;
    return this.request<SysDataScopeDetailDto>('get', _url);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<void> {
    const _url = `/api/SysDataScope/${id}`;
    return this.request<void>('delete', _url);
  }
}