import { ResourceValueDto } from '../resource-mod/resource-value-dto.model';

export interface ResourceAddDto {
  /** environmentId */
  environmentId: string;
  /** categoryId */
  categoryId: string;
  /** groupId */
  groupId?: string | null;
  /** definitionId */
  definitionId: string;
  /** tagNames */
  tagNames: string[];
  /** values */
  values: ResourceValueDto[];
}
