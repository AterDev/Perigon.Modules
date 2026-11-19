import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseService } from '../base.service';
import { SysUserOptionDto } from '../models/system-mod/sys-user-option-dto.model';

@Injectable({ providedIn: 'root' })
export class SysUserOptionsService extends BaseService {
  search(term: string): Observable<SysUserOptionDto[]> {
    const params = new URLSearchParams({ search: term });
    return this.request('get', `/api/SysUser/options?${params}`);
  }
}
