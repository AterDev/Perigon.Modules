export interface PersonalResourceReviewDto {
  /** environmentId */
  environmentId: string;
  /** categoryId */
  categoryId: string;
  /** groupId */
  groupId?: string | null;
  /** tagNames */
  tagNames: string[];
  /** reviewComment */
  reviewComment?: string | null;
}
