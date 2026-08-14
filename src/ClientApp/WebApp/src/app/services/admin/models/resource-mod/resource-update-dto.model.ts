import { ResourceValueDto } from 'src/app/services/admin/models/resource-mod/resource-value-dto.model';

export interface ResourceUpdateDto {
  /** environmentId */
  environmentId?: string | null;
  /** categoryId */
  categoryId?: string | null;
  /** groupId */
  groupId?: string | null;
  /** definitionId */
  definitionId?: string | null;
  /** tagNames */
  tagNames?: string[] | null;
  /** values */
  values?: ResourceValueDto[] | null;
}
