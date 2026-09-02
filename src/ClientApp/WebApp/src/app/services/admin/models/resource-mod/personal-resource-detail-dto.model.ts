import { PersonalResourceItemDto } from './personal-resource-item-dto.model';
import { PersonalResourceValueDetailDto } from './personal-resource-value-detail-dto.model';

export interface PersonalResourceDetailDto extends PersonalResourceItemDto {
  /** values */
  values: PersonalResourceValueDetailDto[];
}
