import { ResourceDetailDto } from '../resource-mod/resource-detail-dto.model';

/**
 * 用户收藏资源详情。
 */
export interface UserFavoriteResourceDetailDto {
  /** 收藏记录 ID。 */
  id: string;
  /** 被收藏的常规资源 ID。 */
  resourceId: string;
  /** 收藏时间。 */
  createdTime: Date;
  /** 资源详情响应结构，包含资源的动态属性值。 */
  resource: ResourceDetailDto;
}
