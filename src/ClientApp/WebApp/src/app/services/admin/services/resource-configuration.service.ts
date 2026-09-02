import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResEnvironment } from '../models/entity/res-environment.model';
import { ResEnvironmentAddDto } from '../models/resource-mod/res-environment-add-dto.model';
import { ResEnvironmentUpdateDto } from '../models/resource-mod/res-environment-update-dto.model';
import { ResCategory } from '../models/entity/res-category.model';
import { ResCategoryAddDto } from '../models/resource-mod/res-category-add-dto.model';
import { ResCategoryUpdateDto } from '../models/resource-mod/res-category-update-dto.model';
import { ResGroup } from '../models/entity/res-group.model';
import { ResGroupAddDto } from '../models/resource-mod/res-group-add-dto.model';
import { ResGroupUpdateDto } from '../models/resource-mod/res-group-update-dto.model';
import { ResTag } from '../models/entity/res-tag.model';
import { ResTagAddDto } from '../models/resource-mod/res-tag-add-dto.model';
import { ResTagUpdateDto } from '../models/resource-mod/res-tag-update-dto.model';
import { ResDefinitionProperty } from '../models/entity/res-definition-property.model';
import { ResDefinitionPropertyAddDto } from '../models/resource-mod/res-definition-property-add-dto.model';
import { ResDefinitionPropertyUpdateDto } from '../models/resource-mod/res-definition-property-update-dto.model';
import { ResDefinition } from '../models/entity/res-definition.model';
import { ResDefinitionAddDto } from '../models/resource-mod/res-definition-add-dto.model';
import { ResDefinitionUpdateDto } from '../models/resource-mod/res-definition-update-dto.model';
import { ResPermission } from '../models/entity/res-permission.model';
import { ResPermissionUpdateDto } from '../models/resource-mod/res-permission-update-dto.model';
/**
 * 资源基础配置管理，包括环境、分类、分组、标签、属性定义、资源定义和资源权限。
 */
@Injectable({ providedIn: 'root' })
export class ResourceConfigurationService extends BaseService {
  /**
   * 获取当前租户的资源环境列表。
   */
  environments(): Observable<ResEnvironment[]> {
    const _url = `/api/ResourceConfiguration/environments`;
    return this.request<ResEnvironment[]>('get', _url);
  }
  /**
   * 新增资源环境。
   * @param data ResEnvironmentAddDto
   */
  addEnvironment(data: ResEnvironmentAddDto): Observable<ResEnvironment> {
    const _url = `/api/ResourceConfiguration/environments`;
    return this.request<ResEnvironment>('post', _url, data);
  }
  /**
   * 更新资源环境。
   * @param id 资源环境唯一标识。
   * @param data ResEnvironmentUpdateDto
   */
  updateEnvironment(id: string, data: ResEnvironmentUpdateDto): Observable<ResEnvironment> {
    const _url = `/api/ResourceConfiguration/environments/${id}`;
    return this.request<ResEnvironment>('put', _url, data);
  }
  /**
   * 删除资源环境。
   * @param id 资源环境唯一标识。
   */
  deleteEnvironment(id: string): Observable<void> {
    const _url = `/api/ResourceConfiguration/environments/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 获取当前租户的资源分类列表。
   */
  categories(): Observable<ResCategory[]> {
    const _url = `/api/ResourceConfiguration/categories`;
    return this.request<ResCategory[]>('get', _url);
  }
  /**
   * 新增资源分类。
   * @param data ResCategoryAddDto
   */
  addCategory(data: ResCategoryAddDto): Observable<ResCategory> {
    const _url = `/api/ResourceConfiguration/categories`;
    return this.request<ResCategory>('post', _url, data);
  }
  /**
   * 更新资源分类。
   * @param id 资源分类唯一标识。
   * @param data ResCategoryUpdateDto
   */
  updateCategory(id: string, data: ResCategoryUpdateDto): Observable<ResCategory> {
    const _url = `/api/ResourceConfiguration/categories/${id}`;
    return this.request<ResCategory>('put', _url, data);
  }
  /**
   * 删除资源分类。
   * @param id 资源分类唯一标识。
   */
  deleteCategory(id: string): Observable<void> {
    const _url = `/api/ResourceConfiguration/categories/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 获取资源分组列表。
   * @param categoryId 可选的资源分类标识；指定后仅返回该分类下的分组。
   */
  groups(categoryId: string | null): Observable<ResGroup[]> {
    const _url = `/api/ResourceConfiguration/groups?categoryId=${categoryId ?? ''}`;
    return this.request<ResGroup[]>('get', _url);
  }
  /**
   * 新增资源分组。
   * @param data ResGroupAddDto
   */
  addGroup(data: ResGroupAddDto): Observable<ResGroup> {
    const _url = `/api/ResourceConfiguration/groups`;
    return this.request<ResGroup>('post', _url, data);
  }
  /**
   * 更新资源分组。
   * @param id 资源分组唯一标识。
   * @param data ResGroupUpdateDto
   */
  updateGroup(id: string, data: ResGroupUpdateDto): Observable<ResGroup> {
    const _url = `/api/ResourceConfiguration/groups/${id}`;
    return this.request<ResGroup>('put', _url, data);
  }
  /**
   * 删除资源分组。
   * @param id 资源分组唯一标识。
   */
  deleteGroup(id: string): Observable<void> {
    const _url = `/api/ResourceConfiguration/groups/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 获取当前租户的资源标签列表。
   */
  tags(): Observable<ResTag[]> {
    const _url = `/api/ResourceConfiguration/tags`;
    return this.request<ResTag[]>('get', _url);
  }
  /**
   * 新增资源标签。
   * @param data ResTagAddDto
   */
  addTag(data: ResTagAddDto): Observable<ResTag> {
    const _url = `/api/ResourceConfiguration/tags`;
    return this.request<ResTag>('post', _url, data);
  }
  /**
   * 更新资源标签。
   * @param id 资源标签唯一标识。
   * @param data ResTagUpdateDto
   */
  updateTag(id: string, data: ResTagUpdateDto): Observable<ResTag> {
    const _url = `/api/ResourceConfiguration/tags/${id}`;
    return this.request<ResTag>('put', _url, data);
  }
  /**
   * 删除资源标签。
   * @param id 资源标签唯一标识。
   */
  deleteTag(id: string): Observable<void> {
    const _url = `/api/ResourceConfiguration/tags/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 获取资源属性定义列表。
   * @param name 可选的属性名称关键字。
   */
  properties(name: string | null): Observable<ResDefinitionProperty[]> {
    const _url = `/api/ResourceConfiguration/properties?name=${name ?? ''}`;
    return this.request<ResDefinitionProperty[]>('get', _url);
  }
  /**
   * 新增资源属性定义。
   * @param data ResDefinitionPropertyAddDto
   */
  addProperty(data: ResDefinitionPropertyAddDto): Observable<ResDefinitionProperty> {
    const _url = `/api/ResourceConfiguration/properties`;
    return this.request<ResDefinitionProperty>('post', _url, data);
  }
  /**
   * 更新资源属性定义。
   * @param id 资源属性定义唯一标识。
   * @param data ResDefinitionPropertyUpdateDto
   */
  updateProperty(id: string, data: ResDefinitionPropertyUpdateDto): Observable<ResDefinitionProperty> {
    const _url = `/api/ResourceConfiguration/properties/${id}`;
    return this.request<ResDefinitionProperty>('put', _url, data);
  }
  /**
   * 删除资源属性定义。
   * @param id 资源属性定义唯一标识。
   */
  deleteProperty(id: string): Observable<void> {
    const _url = `/api/ResourceConfiguration/properties/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 获取资源定义列表。
   * @param name 可选的资源定义名称关键字。
   */
  definitions(name: string | null): Observable<ResDefinition[]> {
    const _url = `/api/ResourceConfiguration/definitions?name=${name ?? ''}`;
    return this.request<ResDefinition[]>('get', _url);
  }
  /**
   * 新增资源定义。
   * @param data ResDefinitionAddDto
   */
  addDefinition(data: ResDefinitionAddDto): Observable<ResDefinition> {
    const _url = `/api/ResourceConfiguration/definitions`;
    return this.request<ResDefinition>('post', _url, data);
  }
  /**
   * 更新资源定义。
   * @param id 资源定义唯一标识。
   * @param data ResDefinitionUpdateDto
   */
  updateDefinition(id: string, data: ResDefinitionUpdateDto): Observable<ResDefinition> {
    const _url = `/api/ResourceConfiguration/definitions/${id}`;
    return this.request<ResDefinition>('put', _url, data);
  }
  /**
   * 删除资源定义。
   * @param id 资源定义唯一标识。
   */
  deleteDefinition(id: string): Observable<void> {
    const _url = `/api/ResourceConfiguration/definitions/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 获取指定环境和分类的资源权限。
   * @param environmentId 资源环境唯一标识。
   * @param categoryId 资源分类唯一标识。
   */
  permissions(environmentId: string | null, categoryId: string | null): Observable<ResPermission[]> {
    const _url = `/api/ResourceConfiguration/permissions?environmentId=${environmentId ?? ''}&categoryId=${categoryId ?? ''}`;
    return this.request<ResPermission[]>('get', _url);
  }
  /**
   * 替换指定环境和分类的资源权限。
   * @param data ResPermissionUpdateDto
   */
  setPermissions(data: ResPermissionUpdateDto): Observable<void> {
    const _url = `/api/ResourceConfiguration/permissions`;
    return this.request<void>('put', _url, data);
  }
}