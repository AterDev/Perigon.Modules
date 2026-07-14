import { ResDefinitionProperty } from '../entity/res-definition-property.model';
import { Resource } from '../entity/resource.model';

export interface ResDefinition {
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
  /** name */
  name: string;
  /** icon */
  icon?: string | null;
  /** properties */
  properties: ResDefinitionProperty[];
  /** resources */
  resources: Resource[];
}
