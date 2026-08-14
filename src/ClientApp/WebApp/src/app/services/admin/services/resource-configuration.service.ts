import { BaseService } from 'src/app/services/admin/base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResEnvironment } from 'src/app/services/admin/models/entity/res-environment.model';
import { ResEnvironmentAddDto } from 'src/app/services/admin/models/resource-mod/res-environment-add-dto.model';
import { ResEnvironmentUpdateDto } from 'src/app/services/admin/models/resource-mod/res-environment-update-dto.model';
import { ResCategory } from 'src/app/services/admin/models/entity/res-category.model';
import { ResCategoryAddDto } from 'src/app/services/admin/models/resource-mod/res-category-add-dto.model';
import { ResCategoryUpdateDto } from 'src/app/services/admin/models/resource-mod/res-category-update-dto.model';
import { ResGroup } from 'src/app/services/admin/models/entity/res-group.model';
import { ResGroupAddDto } from 'src/app/services/admin/models/resource-mod/res-group-add-dto.model';
import { ResGroupUpdateDto } from 'src/app/services/admin/models/resource-mod/res-group-update-dto.model';
import { ResTag } from 'src/app/services/admin/models/entity/res-tag.model';
import { ResTagAddDto } from 'src/app/services/admin/models/resource-mod/res-tag-add-dto.model';
import { ResTagUpdateDto } from 'src/app/services/admin/models/resource-mod/res-tag-update-dto.model';
import { ResDefinitionProperty } from 'src/app/services/admin/models/entity/res-definition-property.model';
import { ResDefinitionPropertyAddDto } from 'src/app/services/admin/models/resource-mod/res-definition-property-add-dto.model';
import { ResDefinitionPropertyUpdateDto } from 'src/app/services/admin/models/resource-mod/res-definition-property-update-dto.model';
import { ResDefinition } from 'src/app/services/admin/models/entity/res-definition.model';
import { ResDefinitionAddDto } from 'src/app/services/admin/models/resource-mod/res-definition-add-dto.model';
import { ResDefinitionUpdateDto } from 'src/app/services/admin/models/resource-mod/res-definition-update-dto.model';
import { ResPermission } from 'src/app/services/admin/models/entity/res-permission.model';
import { ResPermissionUpdateDto } from 'src/app/services/admin/models/resource-mod/res-permission-update-dto.model';
/**
 * 
 */
@Injectable({ providedIn: 'root' })
export class ResourceConfigurationService extends BaseService {
  /**
   * environments
   */
  environments(): Observable<ResEnvironment[]> {
    const _url = `/api/ResourceConfiguration/environments`;
    return this.request<ResEnvironment[]>('get', _url);
  }
  /**
   * addEnvironment
   * @param data ResEnvironmentAddDto
   */
  addEnvironment(data: ResEnvironmentAddDto): Observable<ResEnvironment> {
    const _url = `/api/ResourceConfiguration/environments`;
    return this.request<ResEnvironment>('post', _url, data);
  }
  /**
   * updateEnvironment
   * @param id string
   * @param data ResEnvironmentUpdateDto
   */
  updateEnvironment(id: string, data: ResEnvironmentUpdateDto): Observable<ResEnvironment> {
    const _url = `/api/ResourceConfiguration/environments/${id}`;
    return this.request<ResEnvironment>('put', _url, data);
  }
  /**
   * deleteEnvironment
   * @param id string
   */
  deleteEnvironment(id: string): Observable<any> {
    const _url = `/api/ResourceConfiguration/environments/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * categories
   */
  categories(): Observable<ResCategory[]> {
    const _url = `/api/ResourceConfiguration/categories`;
    return this.request<ResCategory[]>('get', _url);
  }
  /**
   * addCategory
   * @param data ResCategoryAddDto
   */
  addCategory(data: ResCategoryAddDto): Observable<ResCategory> {
    const _url = `/api/ResourceConfiguration/categories`;
    return this.request<ResCategory>('post', _url, data);
  }
  /**
   * updateCategory
   * @param id string
   * @param data ResCategoryUpdateDto
   */
  updateCategory(id: string, data: ResCategoryUpdateDto): Observable<ResCategory> {
    const _url = `/api/ResourceConfiguration/categories/${id}`;
    return this.request<ResCategory>('put', _url, data);
  }
  /**
   * deleteCategory
   * @param id string
   */
  deleteCategory(id: string): Observable<any> {
    const _url = `/api/ResourceConfiguration/categories/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * groups
   * @param categoryId string
   */
  groups(categoryId: string | null): Observable<ResGroup[]> {
    const _url = `/api/ResourceConfiguration/groups?categoryId=${categoryId ?? ''}`;
    return this.request<ResGroup[]>('get', _url);
  }
  /**
   * addGroup
   * @param data ResGroupAddDto
   */
  addGroup(data: ResGroupAddDto): Observable<ResGroup> {
    const _url = `/api/ResourceConfiguration/groups`;
    return this.request<ResGroup>('post', _url, data);
  }
  /**
   * updateGroup
   * @param id string
   * @param data ResGroupUpdateDto
   */
  updateGroup(id: string, data: ResGroupUpdateDto): Observable<ResGroup> {
    const _url = `/api/ResourceConfiguration/groups/${id}`;
    return this.request<ResGroup>('put', _url, data);
  }
  /**
   * deleteGroup
   * @param id string
   */
  deleteGroup(id: string): Observable<any> {
    const _url = `/api/ResourceConfiguration/groups/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * tags
   */
  tags(): Observable<ResTag[]> {
    const _url = `/api/ResourceConfiguration/tags`;
    return this.request<ResTag[]>('get', _url);
  }
  /**
   * addTag
   * @param data ResTagAddDto
   */
  addTag(data: ResTagAddDto): Observable<ResTag> {
    const _url = `/api/ResourceConfiguration/tags`;
    return this.request<ResTag>('post', _url, data);
  }
  /**
   * updateTag
   * @param id string
   * @param data ResTagUpdateDto
   */
  updateTag(id: string, data: ResTagUpdateDto): Observable<ResTag> {
    const _url = `/api/ResourceConfiguration/tags/${id}`;
    return this.request<ResTag>('put', _url, data);
  }
  /**
   * deleteTag
   * @param id string
   */
  deleteTag(id: string): Observable<any> {
    const _url = `/api/ResourceConfiguration/tags/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * properties
   * @param name string
   */
  properties(name: string | null): Observable<ResDefinitionProperty[]> {
    const _url = `/api/ResourceConfiguration/properties?name=${encodeURIComponent(name ?? '')}`;
    return this.request<ResDefinitionProperty[]>('get', _url);
  }
  /**
   * addProperty
   * @param data ResDefinitionPropertyAddDto
   */
  addProperty(data: ResDefinitionPropertyAddDto): Observable<ResDefinitionProperty> {
    const _url = `/api/ResourceConfiguration/properties`;
    return this.request<ResDefinitionProperty>('post', _url, data);
  }
  /**
   * updateProperty
   * @param id string
   * @param data ResDefinitionPropertyUpdateDto
   */
  updateProperty(id: string, data: ResDefinitionPropertyUpdateDto): Observable<ResDefinitionProperty> {
    const _url = `/api/ResourceConfiguration/properties/${id}`;
    return this.request<ResDefinitionProperty>('put', _url, data);
  }
  /**
   * deleteProperty
   * @param id string
   */
  deleteProperty(id: string): Observable<any> {
    const _url = `/api/ResourceConfiguration/properties/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * definitions
   * @param name string
   */
  definitions(name: string | null): Observable<ResDefinition[]> {
    const _url = `/api/ResourceConfiguration/definitions?name=${name ?? ''}`;
    return this.request<ResDefinition[]>('get', _url);
  }
  /**
   * addDefinition
   * @param data ResDefinitionAddDto
   */
  addDefinition(data: ResDefinitionAddDto): Observable<ResDefinition> {
    const _url = `/api/ResourceConfiguration/definitions`;
    return this.request<ResDefinition>('post', _url, data);
  }
  /**
   * updateDefinition
   * @param id string
   * @param data ResDefinitionUpdateDto
   */
  updateDefinition(id: string, data: ResDefinitionUpdateDto): Observable<ResDefinition> {
    const _url = `/api/ResourceConfiguration/definitions/${id}`;
    return this.request<ResDefinition>('put', _url, data);
  }
  /**
   * deleteDefinition
   * @param id string
   */
  deleteDefinition(id: string): Observable<any> {
    const _url = `/api/ResourceConfiguration/definitions/${id}`;
    return this.request<any>('delete', _url);
  }
  /**
   * permissions
   * @param environmentId string
   * @param categoryId string
   */
  permissions(environmentId: string | null, categoryId: string | null): Observable<ResPermission[]> {
    const _url = `/api/ResourceConfiguration/permissions?environmentId=${environmentId ?? ''}&categoryId=${categoryId ?? ''}`;
    return this.request<ResPermission[]>('get', _url);
  }
  /**
   * setPermissions
   * @param data ResPermissionUpdateDto
   */
  setPermissions(data: ResPermissionUpdateDto): Observable<any> {
    const _url = `/api/ResourceConfiguration/permissions`;
    return this.request<any>('put', _url, data);
  }
}
