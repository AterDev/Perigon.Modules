import { ResValueType } from '../entity/res-value-type.model';
import { ResDefinition } from '../entity/res-definition.model';
import { ResValue } from '../entity/res-value.model';

export interface ResDefinitionProperty {
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
  /** valueType */
  valueType: ResValueType;
  /** isRequired */
  isRequired: boolean;
  /** maxLength */
  maxLength: number;
  /** sort */
  sort: number;
  /** definitionId */
  definitionId: string;
  /** definition */
  definition: ResDefinition;
  /** values */
  values: ResValue[];
}
