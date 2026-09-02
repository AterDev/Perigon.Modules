import { ResDefinitionPropertyDto } from '../resource-mod/res-definition-property-dto.model';

/**
 * 资源定义新增请求结构。
 */
export interface ResDefinitionAddDto {
  /** 资源定义名称。 */
  name: string;
  /** Material Icons 图标名称，可选。 */
  icon?: string | null;
  /** 资源定义包含的属性配置，按 Sort 排序。 */
  properties: ResDefinitionPropertyDto[];
}
