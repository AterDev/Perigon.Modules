import { ResEnvironment } from '../entity/res-environment.model';
import { ResCategory } from '../entity/res-category.model';
import { ResGroup } from '../entity/res-group.model';
import { ResDefinition } from '../entity/res-definition.model';
import { ResValue } from '../entity/res-value.model';

export interface Resource {
  /** id */
  id: string;
  /** createdTime */
  createdTime: Date;
  /** updatedTime */
  updatedTime: Date;
  /** isDeleted */
  isDeleted: boolean;
  /** tenantId */
  tenantId: string;
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
  /** environment */
  environment: ResEnvironment;
  /** category */
  category: ResCategory;
  /** group */
  group: ResGroup;
  /** definition */
  definition: ResDefinition;
  /** values */
  values: ResValue[];
}
