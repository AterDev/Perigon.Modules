import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { UserFavoriteResourceItemDto } from '../models/resource-mod/user-favorite-resource-item-dto.model';
import { UserFavoriteResourceDetailDto } from '../models/resource-mod/user-favorite-resource-detail-dto.model';
import { UserFavoriteResourceAddDto } from '../models/resource-mod/user-favorite-resource-add-dto.model';
import { UserFavoriteResourceCreatedDto } from '../models/resource-mod/user-favorite-resource-created-dto.model';
/**
 * 用户收藏资源接口。
 */
@Injectable({ providedIn: 'root' })
export class UserFavoriteResourceService extends BaseService {
  /**
   * 查询当前用户的收藏资源。
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  mine(pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<UserFavoriteResourceItemDto>> {
    const _url = `/api/UserFavoriteResource/mine?pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<UserFavoriteResourceItemDto>>('get', _url);
  }
  /**
   * 查询当前用户对指定资源的收藏详情。
   * @param resourceId string
   */
  detail(resourceId: string): Observable<UserFavoriteResourceDetailDto> {
    const _url = `/api/UserFavoriteResource/${resourceId}`;
    return this.request<UserFavoriteResourceDetailDto>('get', _url);
  }
  /**
   * 取消当前用户对指定资源的收藏。
   * @param resourceId string
   */
  remove(resourceId: string): Observable<boolean> {
    const _url = `/api/UserFavoriteResource/${resourceId}`;
    return this.request<boolean>('delete', _url);
  }
  /**
   * 收藏一个当前用户可见的常规资源。
   * @param data UserFavoriteResourceAddDto
   */
  add(data: UserFavoriteResourceAddDto): Observable<UserFavoriteResourceCreatedDto> {
    const _url = `/api/UserFavoriteResource`;
    return this.request<UserFavoriteResourceCreatedDto>('post', _url, data);
  }
}