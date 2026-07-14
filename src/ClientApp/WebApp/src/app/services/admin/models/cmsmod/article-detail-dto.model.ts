import { LanguageType } from '../entity/language-type.model';
import { ContentType } from '../entity/content-type.model';
import { ArticleCategory } from '../entity/article-category.model';

/**
 * 博客概要
 */
export interface ArticleDetailDto {
  /** 标题 */
  title: string;
  /** 描述 */
  description?: string | null;
  /** 内容 */
  content: string;
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
  /** 目录 */
  catalog: ArticleCategory;
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
