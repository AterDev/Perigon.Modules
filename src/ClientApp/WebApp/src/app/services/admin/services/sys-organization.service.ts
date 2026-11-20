import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseService } from '../base.service';
import { SysOrganization } from '../models/entity/sys-organization.model';
import { SysOrganizationAddDto } from '../models/system-mod/sys-organization-add-dto.model';
import { SysOrganizationUpdateDto } from '../models/system-mod/sys-organization-update-dto.model';

/** 系统组织结构管理接口。 */
@Injectable({ providedIn: 'root' })
export class SysOrganizationService extends BaseService {
  /** 获取组织树。 */
  list(): Observable<SysOrganization[]> {
    return this.request<SysOrganization[]>('get', '/api/SysOrganization');
  }

  /** 新增组织。 */
  add(data: SysOrganizationAddDto): Observable<SysOrganization> {
    return this.request<SysOrganization>('post', '/api/SysOrganization', data);
  }

  /** 更新组织。 */
  update(
    id: string,
    data: SysOrganizationUpdateDto,
  ): Observable<SysOrganization> {
    return this.request<SysOrganization>(
      'patch',
      `/api/SysOrganization/${id}`,
      data,
    );
  }

  /** 删除组织。 */
  delete(id: string): Observable<void> {
    return this.request<void>('delete', `/api/SysOrganization/${id}`);
  }
}
