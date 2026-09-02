import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PageList } from '../models/perigon/page-list.model';
import { LanguageType } from '../models/entity/language-type.model';
import { ContentType } from '../models/entity/content-type.model';
import { ArticleItemDto } from '../models/cmsmod/article-item-dto.model';
import { ArticleAddDto } from '../models/cmsmod/article-add-dto.model';
import { Article } from '../models/entity/article.model';
import { ArticleImageUploadDto } from '../models/cmsmod/article-image-upload-dto.model';
import { ArticleDetailDto } from '../models/cmsmod/article-detail-dto.model';
import { ArticleUpdateDto } from '../models/cmsmod/article-update-dto.model';
/**
 * 文章管理。
 */
@Injectable({ providedIn: 'root' })
export class ArticleService extends BaseService {
  /**
   * 分页查询文章列表。
   * @param title 按文章标题筛选。
   * @param description 按文章描述筛选。
   * @param authors 按作者名称筛选。
   * @param translateTitle 按翻译后的文章标题筛选。
   * @param languageType 按文章语言类型筛选。
   * @param blogType 按文章内容类型筛选。
   * @param isAudit 按是否审核筛选。
   * @param isPublic 按是否公开筛选。
   * @param isOriginal 按是否原创筛选。
   * @param userId 按作者用户 ID 筛选。
   * @param catalogId 按所属目录 ID 筛选。
   * @param viewCount 按浏览量筛选。
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  list(title: string | null, description: string | null, authors: string | null, translateTitle: string | null, languageType: LanguageType | null, blogType: ContentType | null, isAudit: boolean | null, isPublic: boolean | null, isOriginal: boolean | null, userId: string | null, catalogId: string | null, viewCount: number | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<ArticleItemDto>> {
    const _url = `/api/Article/list?title=${title ?? ''}&description=${description ?? ''}&authors=${authors ?? ''}&translateTitle=${translateTitle ?? ''}&languageType=${languageType ?? ''}&blogType=${blogType ?? ''}&isAudit=${isAudit ?? ''}&isPublic=${isPublic ?? ''}&isOriginal=${isOriginal ?? ''}&userId=${userId ?? ''}&catalogId=${catalogId ?? ''}&viewCount=${viewCount ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<ArticleItemDto>>('get', _url);
  }
  /**
   * 新增文章。
   * @param data ArticleAddDto
   */
  add(data: ArticleAddDto): Observable<Article> {
    const _url = `/api/Article`;
    return this.request<Article>('post', _url, data);
  }
  /**
   * 上传文章图片。
   * @param file 要上传的图片文件。
   */
  uploadImage(file: File | null): Observable<ArticleImageUploadDto> {
    const formData = new FormData();
    if (file !== null && file !== undefined) formData.append('file', file, file.name);
    const _url = `/api/Article/images`;
    return this.request<ArticleImageUploadDto>('post', _url, formData);
  }
  /**
   * 获取文章详情。
   * @param id 文章唯一标识。
   */
  detail(id: string): Observable<ArticleDetailDto> {
    const _url = `/api/Article/${id}`;
    return this.request<ArticleDetailDto>('get', _url);
  }
  /**
   * 更新文章。
   * @param id 文章唯一标识。
   * @param data ArticleUpdateDto
   */
  update(id: string, data: ArticleUpdateDto): Observable<Article> {
    const _url = `/api/Article/${id}`;
    return this.request<Article>('patch', _url, data);
  }
  /**
   * 删除文章。
   * @param id 文章唯一标识。
   */
  delete(id: string): Observable<void> {
    const _url = `/api/Article/${id}`;
    return this.request<void>('delete', _url);
  }
}