import { ResourceItemDto } from '../resource-mod/resource-item-dto.model';

/**
 * 我的收藏资源列表项。
 */
export interface UserFavoriteResourceItemDto {
  /** 收藏记录 ID。 */
  id: string;
  /** 被收藏的常规资源 ID。 */
  resourceId: string;
  /** 收藏时间。 */
  createdTime: Date;
  /** 资源列表项响应结构。 */
  resource: ResourceItemDto;
}
