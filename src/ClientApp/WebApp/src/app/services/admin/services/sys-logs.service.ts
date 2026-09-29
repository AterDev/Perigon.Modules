import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SysLogsFilterDto } from '../models/system-mod/sys-logs-filter-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SysLogsItemDto } from '../models/system-mod/sys-logs-item-dto.model';
/**
 * 系统日志
 */
@Injectable({ providedIn: 'root' })
export class SysLogsService extends BaseService {
  /**
   * 筛选 ✅
   * @param data SysLogsFilterDto
   */
  filter(data: SysLogsFilterDto): Observable<PageList<SysLogsItemDto>> {
    const _url = `/api/SysLogs/filter`;
    return this.request<PageList<SysLogsItemDto>>('post', _url, data);
  }
}