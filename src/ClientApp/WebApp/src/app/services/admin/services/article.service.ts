import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { LanguageType } from '../models/entity/language-type.model';
import { ContentType } from '../models/entity/content-type.model';
import { ArticleItemDto } from '../models/cmsmod/article-item-dto.model';
import { ArticleAddDto } from '../models/cmsmod/article-add-dto.model';
import { Article } from '../models/entity/article.model';
import { ArticleDetailDto } from '../models/cmsmod/article-detail-dto.model';
import { ArticleUpdateDto } from '../models/cmsmod/article-update-dto.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class ArticleService extends BaseService {
  /**
   * list
   * @param title 标题
   * @param description 描述
   * @param authors 作者
   * @param translateTitle 标题
   * @param languageType 语言类型
   * @param blogType 全站类别
   * @param isAudit 是否审核
   * @param isPublic 是否公开
   * @param isOriginal 是否原创
   * @param userId string
   * @param catalogId string
   * @param viewCount 浏览量
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(title: string | null, description: string | null, authors: string | null, translateTitle: string | null, languageType: LanguageType | null, blogType: ContentType | null, isAudit: boolean | null, isPublic: boolean | null, isOriginal: boolean | null, userId: string | null, catalogId: string | null, viewCount: number | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<ArticleItemDto>> {
    const _url = `/api/Article/list?title=${title ?? ''}&description=${description ?? ''}&authors=${authors ?? ''}&translateTitle=${translateTitle ?? ''}&languageType=${languageType ?? ''}&blogType=${blogType ?? ''}&isAudit=${isAudit ?? ''}&isPublic=${isPublic ?? ''}&isOriginal=${isOriginal ?? ''}&userId=${userId ?? ''}&catalogId=${catalogId ?? ''}&viewCount=${viewCount ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<ArticleItemDto>>('get', _url);
  }
  /**
   * add
   * @param data ArticleAddDto
   */
  add(data: ArticleAddDto): Observable<Article> {
    const _url = `/api/Article`;
    return this.request<Article>('post', _url, data);
  }
  /**
   * detail
   * @param id string
   */
  detail(id: string): Observable<ArticleDetailDto> {
    const _url = `/api/Article/${id}`;
    return this.request<ArticleDetailDto>('get', _url);
  }
  /**
   * update
   * @param id string
   * @param data ArticleUpdateDto
   */
  update(id: string, data: ArticleUpdateDto): Observable<Article> {
    const _url = `/api/Article/${id}`;
    return this.request<Article>('patch', _url, data);
  }
  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<any> {
    const _url = `/api/Article/${id}`;
    return this.request<any>('delete', _url);
  }
}