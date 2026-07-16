import { ResourceValueInput } from '../resource-mod/resource-value-input.model';

export interface ResourceInput {
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
  values: ResourceValueInput[];
}
