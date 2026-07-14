import { LanguageType } from '../entity/language-type.model';
import { ContentType } from '../entity/content-type.model';

/**
 * 博客更新时请求结构
 */
export interface ArticleUpdateDto {
  /** 标题 */
  title?: string | null;
  /** 描述 */
  description?: string | null;
  /** 内容 */
  content?: string | null;
  /** 作者 */
  authors?: string | null;
  /** 标题 */
  translateTitle?: string | null;
  /** 翻译内容 */
  translateContent?: string | null;
  /** languageType */
  languageType?: LanguageType | null;
  /** blogType */
  blogType?: ContentType | null;
  /** 是否审核 */
  isAudit?: boolean | null;
  /** 是否公开 */
  isPublic?: boolean | null;
  /** 是否原创 */
  isOriginal?: boolean | null;
  /** userId */
  userId?: string | null;
  /** catalogId */
  catalogId?: string | null;
  /** 浏览量 */
  viewCount?: number | null;
}
