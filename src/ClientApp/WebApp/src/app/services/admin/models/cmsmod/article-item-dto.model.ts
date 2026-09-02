import { LanguageType } from '../entity/language-type.model';
import { ContentType } from '../entity/content-type.model';

/**
 * 文章列表项。
 */
export interface ArticleItemDto {
  /** 文章标题。 */
  title: string;
  /** 文章描述。 */
  description?: string | null;
  /** 作者名称。 */
  authors: string;
  /** 翻译后的文章标题。 */
  translateTitle?: string | null;
  /** languageType */
  languageType: LanguageType;
  /** blogType */
  blogType: ContentType;
  /** 是否已审核。 */
  isAudit: boolean;
  /** 是否公开文章。 */
  isPublic: boolean;
  /** 是否为原创文章。 */
  isOriginal: boolean;
  /** 作者用户 ID。 */
  userId: string;
  /** 所属目录 ID。 */
  catalogId: string;
  /** 浏览量。 */
  viewCount: number;
  /** 文章唯一标识。 */
  id: string;
  /** 创建时间。 */
  createdTime: Date;
  /** 最后更新时间。 */
  updatedTime: Date;
}
