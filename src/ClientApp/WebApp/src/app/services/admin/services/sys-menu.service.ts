import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SysMenuFilterDto } from '../models/system-mod/sys-menu-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SysMenu } from '../models/entity/sys-menu.model';
import { SysMenuAddDto } from '../models/system-mod/sys-menu-add-dto.model';
import { SysMenuUpdateDto } from '../models/system-mod/sys-menu-update-dto.model';
/**
 * 系统菜单
 */
@Injectable({ providedIn: 'root' })
export class SysMenuService extends BaseService {
  /**
   * 筛选 ✅
   * @param data SysMenuFilterDto
   */
  filter(data: SysMenuFilterDto): Observable<PageList<SysMenu>> {
    const _url = `/api/SysMenu/filter`;
    return this.request<PageList<SysMenu>>('post', _url, data);
  }
  /**
   * 新增 ✅
   * @param data SysMenuAddDto
   */
  add(data: SysMenuAddDto): Observable<SysMenu> {
    const _url = `/api/SysMenu`;
    return this.request<SysMenu>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SysMenuUpdateDto
   */
  update(id: string, data: SysMenuUpdateDto): Observable<boolean> {
    const _url = `/api/SysMenu/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SysMenu> {
    const _url = `/api/SysMenu/${id}`;
    return this.request<SysMenu>('get', _url);
  }
}