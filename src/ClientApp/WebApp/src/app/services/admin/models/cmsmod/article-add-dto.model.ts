import { LanguageType } from '../entity/language-type.model';
import { ContentType } from '../entity/content-type.model';

/**
 * 文章添加时请求结构。
 */
export interface ArticleAddDto {
  /** 文章标题。 */
  title: string;
  /** 文章描述。 */
  description?: string | null;
  /** 文章正文内容。 */
  content: string;
  /** 翻译后的文章标题。 */
  translateTitle?: string | null;
  /** 翻译后的文章正文内容。 */
  translateContent?: string | null;
  /** languageType */
  languageType: LanguageType;
  /** blogType */
  blogType: ContentType;
  /** 是否公开文章。 */
  isPublic: boolean;
  /** 是否为原创文章。 */
  isOriginal: boolean;
  /** 所属目录 ID。 */
  catalogId: string;
}
