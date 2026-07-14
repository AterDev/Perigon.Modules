import { ResourceValueDetailDto } from '../resource-mod/resource-value-detail-dto.model';

export interface ResourceDetailDto {
  /** id */
  id: string;
  /** name */
  name: string;
  /** iconUrl */
  iconUrl?: string | null;
  /** environmentId */
  environmentId: string;
  /** environmentName */
  environmentName: string;
  /** categoryId */
  categoryId: string;
  /** categoryName */
  categoryName: string;
  /** groupId */
  groupId?: string | null;
  /** groupName */
  groupName?: string | null;
  /** definitionId */
  definitionId: string;
  /** definitionName */
  definitionName: string;
  /** tagNames */
  tagNames: string[];
  /** updatedTime */
  updatedTime: Date;
  /** description */
  description?: string | null;
  /** values */
  values: ResourceValueDetailDto[];
}
