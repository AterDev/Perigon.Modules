import { PersonalResourceStatus } from './personal-resource-status.model';
import { PersonalResourceValueDto } from './personal-resource-value-dto.model';

export interface PersonalResourceAddDto {
  /** definitionId */
  definitionId: string;
  /** status */
  status: PersonalResourceStatus;
  /** values */
  values: PersonalResourceValueDto[];
}
