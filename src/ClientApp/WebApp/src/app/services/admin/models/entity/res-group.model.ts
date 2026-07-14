import { ResCategory } from '../entity/res-category.model';
import { Resource } from '../entity/resource.model';

export interface ResGroup {
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
  /** description */
  description?: string | null;
  /** icon */
  icon?: string | null;
  /** color */
  color: string;
  /** categoryId */
  categoryId: string;
  /** category */
  category: ResCategory;
  /** resources */
  resources: Resource[];
}
