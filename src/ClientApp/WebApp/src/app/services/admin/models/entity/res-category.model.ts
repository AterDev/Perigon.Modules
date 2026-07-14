import { ResGroup } from '../entity/res-group.model';
import { Resource } from '../entity/resource.model';
import { ResPermission } from '../entity/res-permission.model';

export interface ResCategory {
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
  /** catalogCode */
  catalogCode: string;
  /** icon */
  icon?: string | null;
  /** color */
  color: string;
  /** groups */
  groups: ResGroup[];
  /** resources */
  resources: Resource[];
  /** permissions */
  permissions: ResPermission[];
}
