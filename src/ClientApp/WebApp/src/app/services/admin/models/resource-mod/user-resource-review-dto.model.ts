/**
 * 公开申请审核通过时补充的常规资源信息。
 */
export interface UserResourceReviewDto {
  /** 资源所属环境 ID。 */
  environmentId: string;
  /** 资源所属分类 ID。 */
  categoryId: string;
  /** 资源所属分组 ID，可选。 */
  groupId?: string | null;
  /** 资源标签名称列表。 */
  tagNames: string[];
  /** 审核意见。 */
  reviewComment?: string | null;
}
