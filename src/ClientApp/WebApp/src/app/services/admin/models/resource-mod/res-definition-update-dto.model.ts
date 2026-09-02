import { ResDefinitionPropertyDto } from '../resource-mod/res-definition-property-dto.model';

/**
 * 资源定义更新请求结构。
 */
export interface ResDefinitionUpdateDto {
  /** 资源定义名称。 */
  name?: string | null;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
  /** 资源定义包含的属性配置，按 Sort 排序。 */
  properties?: ResDefinitionPropertyDto[] | null;
}
