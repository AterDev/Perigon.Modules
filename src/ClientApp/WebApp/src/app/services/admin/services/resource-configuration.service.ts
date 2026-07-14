import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResEnvironment } from '../models/entity/res-environment.model';
import { ResEnvironmentInput } from '../models/resource-mod/res-environment-input.model';
import { ResCategory } from '../models/entity/res-category.model';
import { ResCategoryInput } from '../models/resource-mod/res-category-input.model';
import { ResGroup } from '../models/entity/res-group.model';
import { ResGroupInput } from '../models/resource-mod/res-group-input.model';
import { ResTag } from '../models/entity/res-tag.model';
import { ResTagInput } from '../models/resource-mod/res-tag-input.model';
import { ResDefinition } from '../models/entity/res-definition.model';
import { ResDefinitionInput } from '../models/resource-mod/res-definition-input.model';
import { SystemRole } from '../models/entity/system-role.model';
import { ResPermission } from '../models/entity/res-permission.model';
import { ResPermissionInput } from '../models/resource-mod/res-permission-input.model';
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
   * @param data ResEnvironmentInput
   */
  addEnvironment(data: ResEnvironmentInput): Observable<ResEnvironment> {
    const _url = `/api/ResourceConfiguration/environments`;
    return this.request<ResEnvironment>('post', _url, data);
  }
  /**
   * updateEnvironment
   * @param id string
   * @param data ResEnvironmentInput
   */
  updateEnvironment(id: string, data: ResEnvironmentInput): Observable<ResEnvironment> {
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
   * @param data ResCategoryInput
   */
  addCategory(data: ResCategoryInput): Observable<ResCategory> {
    const _url = `/api/ResourceConfiguration/categories`;
    return this.request<ResCategory>('post', _url, data);
  }
  /**
   * updateCategory
   * @param id string
   * @param data ResCategoryInput
   */
  updateCategory(id: string, data: ResCategoryInput): Observable<ResCategory> {
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
   * @param data ResGroupInput
   */
  addGroup(data: ResGroupInput): Observable<ResGroup> {
    const _url = `/api/ResourceConfiguration/groups`;
    return this.request<ResGroup>('post', _url, data);
  }
  /**
   * updateGroup
   * @param id string
   * @param data ResGroupInput
   */
  updateGroup(id: string, data: ResGroupInput): Observable<ResGroup> {
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
   * @param data ResTagInput
   */
  addTag(data: ResTagInput): Observable<ResTag> {
    const _url = `/api/ResourceConfiguration/tags`;
    return this.request<ResTag>('post', _url, data);
  }
  /**
   * updateTag
   * @param id string
   * @param data ResTagInput
   */
  updateTag(id: string, data: ResTagInput): Observable<ResTag> {
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
   * definitions
   */
  definitions(): Observable<ResDefinition[]> {
    const _url = `/api/ResourceConfiguration/definitions`;
    return this.request<ResDefinition[]>('get', _url);
  }
  /**
   * addDefinition
   * @param data ResDefinitionInput
   */
  addDefinition(data: ResDefinitionInput): Observable<ResDefinition> {
    const _url = `/api/ResourceConfiguration/definitions`;
    return this.request<ResDefinition>('post', _url, data);
  }
  /**
   * updateDefinition
   * @param id string
   * @param data ResDefinitionInput
   */
  updateDefinition(id: string, data: ResDefinitionInput): Observable<ResDefinition> {
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
   * roles
   */
  roles(): Observable<SystemRole[]> {
    const _url = `/api/ResourceConfiguration/roles`;
    return this.request<SystemRole[]>('get', _url);
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
   * @param data ResPermissionInput
   */
  setPermissions(data: ResPermissionInput): Observable<any> {
    const _url = `/api/ResourceConfiguration/permissions`;
    return this.request<any>('put', _url, data);
  }
}