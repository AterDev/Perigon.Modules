/**
 * 资源属性值写入请求结构。
 */
export interface ResourceValueDto {
  /** 资源定义属性 ID。 */
  definitionPropertyId: string;
  /** 属性值；保存时会根据属性类型规范化，最大长度为 1000。 */
  value: string;
}
