import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { ArticleCategoryItemDto } from '../models/cmsmod/article-category-item-dto.model';
import { ArticleCategoryAddDto } from '../models/cmsmod/article-category-add-dto.model';
import { ArticleCategory } from '../models/entity/article-category.model';
import { ArticleCategoryUpdateDto } from '../models/cmsmod/article-category-update-dto.model';
import { ArticleCategoryDetailDto } from '../models/cmsmod/article-category-detail-dto.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class ArticleCategoryService extends BaseService {
  /**
   * 获取目录列表 ✅
   * @param name 目录名称
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(name: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<ArticleCategoryItemDto>> {
    const _url = `/api/ArticleCategory/list?name=${name ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<ArticleCategoryItemDto>>('get', _url);
  }
  /**
   * add ✅
   * @param data ArticleCategoryAddDto
   */
  add(data: ArticleCategoryAddDto): Observable<ArticleCategory> {
    const _url = `/api/ArticleCategory`;
    return this.request<ArticleCategory>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data ArticleCategoryUpdateDto
   */
  update(id: string, data: ArticleCategoryUpdateDto): Observable<boolean> {
    const _url = `/api/ArticleCategory/${id}`;
    return this.request<boolean>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  detail(id: string): Observable<ArticleCategoryDetailDto> {
    const _url = `/api/ArticleCategory/${id}`;
    return this.request<ArticleCategoryDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ArticleCategory/${id}`;
    return this.request<boolean>('delete', _url);
  }
}