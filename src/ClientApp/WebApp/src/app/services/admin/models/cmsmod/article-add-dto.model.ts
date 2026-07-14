import { LanguageType } from '../entity/language-type.model';
import { ContentType } from '../entity/content-type.model';

/**
 * 博客添加时请求结构
 */
export interface ArticleAddDto {
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
  /** 翻译内容 */
  translateContent?: string | null;
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
}
