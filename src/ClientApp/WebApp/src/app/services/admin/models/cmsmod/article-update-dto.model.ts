import { LanguageType } from '../entity/language-type.model';
import { ContentType } from '../entity/content-type.model';

/**
 * 文章更新时请求结构。
 */
export interface ArticleUpdateDto {
  /** 文章标题。 */
  title?: string | null;
  /** 文章描述。 */
  description?: string | null;
  /** 文章正文内容。 */
  content?: string | null;
  /** 翻译后的文章标题。 */
  translateTitle?: string | null;
  /** 翻译后的文章正文内容。 */
  translateContent?: string | null;
  /** languageType */
  languageType?: LanguageType | null;
  /** blogType */
  blogType?: ContentType | null;
  /** 是否公开文章；未提供时保留原值。 */
  isPublic?: boolean | null;
  /** 是否为原创文章；未提供时保留原值。 */
  isOriginal?: boolean | null;
  /** 所属目录 ID；未提供时保留原目录。 */
  catalogId?: string | null;
}
