import { ResDefinitionProperty } from '../../../services/admin/models/entity/res-definition-property.model';

export function sortResourceProperties(
  properties: readonly ResDefinitionProperty[],
): ResDefinitionProperty[] {
  return [...properties].sort((left, right) => {
    if (left.isRequired !== right.isRequired) {
      return left.isRequired ? -1 : 1;
    }

    return left.name.localeCompare(right.name, undefined, {
      numeric: true,
      sensitivity: 'base',
    });
  });
}
