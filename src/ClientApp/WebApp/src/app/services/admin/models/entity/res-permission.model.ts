import { ResEnvironment } from '../entity/res-environment.model';
import { ResCategory } from '../entity/res-category.model';

export interface ResPermission {
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
  /** roleId */
  roleId: string;
  /** environmentId */
  environmentId: string;
  /** categoryId */
  categoryId: string;
  /** environment */
  environment: ResEnvironment;
  /** category */
  category: ResCategory;
}
