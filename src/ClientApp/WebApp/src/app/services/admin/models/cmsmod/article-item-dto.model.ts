import { LanguageType } from '../entity/language-type.model';
import { ContentType } from '../entity/content-type.model';

/**
 * 博客列表元素
 */
export interface ArticleItemDto {
  /** 标题 */
  title: string;
  /** 描述 */
  description?: string | null;
  /** 作者 */
  authors: string;
  /** 标题 */
  translateTitle?: string | null;
  /** languageType */
  languageType: LanguageType;
  /** blogType */
  blogType: ContentType;
  /** 是否审核 */
  isAudit: boolean;
  /** 是否公开 */
  isPublic: boolean;
  /** 是否原创 */
  isOriginal: boolean;
  /** userId */
  userId: string;
  /** catalogId */
  catalogId: string;
  /** 浏览量 */
  viewCount: number;
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
}
