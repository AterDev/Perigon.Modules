import { ResValueType } from '../entity/res-value-type.model';
import { Resource } from '../entity/resource.model';
import { ResDefinitionProperty } from '../entity/res-definition-property.model';

export interface ResValue {
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
  /** resourceId */
  resourceId: string;
  /** definitionPropertyId */
  definitionPropertyId: string;
  /** value */
  value: string;
  /** propertyNameSnapshot */
  propertyNameSnapshot: string;
  /** valueTypeSnapshot */
  valueTypeSnapshot: ResValueType;
  /** resource */
  resource: Resource;
  /** definitionProperty */
  definitionProperty: ResDefinitionProperty;
}
