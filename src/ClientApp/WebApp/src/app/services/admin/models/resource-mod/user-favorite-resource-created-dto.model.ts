/**
 * 收藏资源成功后的响应。
 */
export interface UserFavoriteResourceCreatedDto {
  /** 收藏记录 ID。 */
  id: string;
  /** 被收藏的常规资源 ID。 */
  resourceId: string;
}
