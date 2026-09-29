import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { SysRoleItemDto } from '../models/system-mod/sys-role-item-dto.model';
import { SysRoleAddDto } from '../models/system-mod/sys-role-add-dto.model';
import { SysRole } from '../models/entity/sys-role.model';
import { SysRoleUpdateDto } from '../models/system-mod/sys-role-update-dto.model';
import { SysRoleDetailDto } from '../models/system-mod/sys-role-detail-dto.model';
import { SysRoleSetMenusDto } from '../models/system-mod/sys-role-set-menus-dto.model';
/**
 * 系统角色
SystemMod.Managers.SysRoleManager
 */
@Injectable({ providedIn: 'root' })
export class SysRoleService extends BaseService {
  /**
   * 筛选 ✅
   * @param name 角色显示名称
   * @param nameValue 角色名，系统标识
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(name: string | null, nameValue: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<SysRoleItemDto>> {
    const _url = `/api/SysRole?name=${name ?? ''}&nameValue=${nameValue ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<SysRoleItemDto>>('get', _url);
  }
  /**
   * 新增 ✅
   * @param data SysRoleAddDto
   */
  add(data: SysRoleAddDto): Observable<SysRole> {
    const _url = `/api/SysRole`;
    return this.request<SysRole>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SysRoleUpdateDto
   */
  update(id: string, data: SysRoleUpdateDto): Observable<SysRole> {
    const _url = `/api/SysRole/${id}`;
    return this.request<SysRole>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  detail(id: string): Observable<SysRoleDetailDto> {
    const _url = `/api/SysRole/${id}`;
    return this.request<SysRoleDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<void> {
    const _url = `/api/SysRole/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 角色菜单 ✅
   * @param data SysRoleSetMenusDto
   */
  updateMenus(data: SysRoleSetMenusDto): Observable<SysRole> {
    const _url = `/api/SysRole/menus`;
    return this.request<SysRole>('put', _url, data);
  }
}