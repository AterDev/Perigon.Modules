import { Resource } from '../entity/resource.model';
import { ResPermission } from '../entity/res-permission.model';

export interface ResEnvironment {
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
  /** color */
  color: string;
  /** resources */
  resources: Resource[];
  /** permissions */
  permissions: ResPermission[];
}
