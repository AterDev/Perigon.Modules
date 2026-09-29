import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SysConfigFilterDto } from '../models/system-mod/sys-config-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SysConfigItemDto } from '../models/system-mod/sys-config-item-dto.model';
import { EnumDictionary } from '../models/perigon/enum-dictionary.model';
import { SysConfigAddDto } from '../models/system-mod/sys-config-add-dto.model';
import { SysConfig } from '../models/entity/sys-config.model';
import { SysConfigUpdateDto } from '../models/system-mod/sys-config-update-dto.model';
import { SysConfigDetailDto } from '../models/system-mod/sys-config-detail-dto.model';
/**
 * 系统配置
 */
@Injectable({ providedIn: 'root' })
export class SysConfigService extends BaseService {
  /**
   * 获取配置列表 ✅
   * @param data SysConfigFilterDto
   */
  filter(data: SysConfigFilterDto): Observable<PageList<SysConfigItemDto>> {
    const _url = `/api/SysConfig/filter`;
    return this.request<PageList<SysConfigItemDto>>('post', _url, data);
  }
  /**
   * 获取枚举信息 ✅
   */
  getEnumConfigs(): Observable<Record<string, EnumDictionary[]>> {
    const _url = `/api/SysConfig/enum`;
    return this.request<Record<string, EnumDictionary[]>>('get', _url);
  }
  /**
   * 新增 ✅
   * @param data SysConfigAddDto
   */
  add(data: SysConfigAddDto): Observable<SysConfig> {
    const _url = `/api/SysConfig`;
    return this.request<SysConfig>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SysConfigUpdateDto
   */
  update(id: string, data: SysConfigUpdateDto): Observable<boolean> {
    const _url = `/api/SysConfig/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SysConfigDetailDto> {
    const _url = `/api/SysConfig/${id}`;
    return this.request<SysConfigDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SysConfig/${id}`;
    return this.request<boolean>('delete', _url);
  }
}