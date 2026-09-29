import { BaseService } from '../base.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SysLoginDto } from '../models/system-mod/sys-login-dto.model';
import { AccessTokenDto } from '../models/share/access-token-dto.model';
import { UserInfoDto } from '../models/system-mod/user-info-dto.model';
import { PageList } from '../models/perigon/page-list.model';
import { SysUserItemDto } from '../models/system-mod/sys-user-item-dto.model';
import { SysUserAddDto } from '../models/system-mod/sys-user-add-dto.model';
import { SysUser } from '../models/entity/sys-user.model';
import { SysUserUpdateDto } from '../models/system-mod/sys-user-update-dto.model';
import { SysUserDetailDto } from '../models/system-mod/sys-user-detail-dto.model';
/**
 * 系统用户
 */
@Injectable({ providedIn: 'root' })
export class SysUserService extends BaseService {
  /**
   * 登录时，发送邮箱验证码 ✅
   * @param email
   */
  sendVerifyCode(email: string | null): Observable<void> {
    const _url = `/api/SysUser/verifyCode?email=${email ?? ''}`;
    return this.request<void>('post', _url);
  }
  /**
   * 获取图形验证码 ✅
   */
  getCaptchaImage(): Observable<void> {
    const _url = `/api/SysUser/captcha`;
    return this.request<void>('get', _url);
  }
  /**
   * Get AccessToken ✅
   * @param data SysLoginDto
   */
  login(data: SysLoginDto): Observable<AccessTokenDto> {
    const _url = `/api/SysUser/authorize`;
    return this.request<AccessTokenDto>('post', _url, data);
  }
  /**
   * Get UserInfo ✅
   */
  getUserInfo(): Observable<UserInfoDto> {
    const _url = `/api/SysUser/userinfo`;
    return this.request<UserInfoDto>('get', _url);
  }
  /**
   * 刷新 token
   * @param refreshToken
   */
  refreshToken(refreshToken: string | null): Observable<AccessTokenDto> {
    const _url = `/api/SysUser/refresh_token?refreshToken=${refreshToken ?? ''}`;
    return this.request<AccessTokenDto>('get', _url);
  }
  /**
   * 退出 ✅
   * @param id string
   */
  logout(id: string): Observable<boolean> {
    const _url = `/api/SysUser/logout/${id}`;
    return this.request<boolean>('post', _url);
  }
  /**
   * 筛选 ✅
   * @param userName 用户名
   * @param roleId 角色id
   * @param pageIndex number
   * @param pageSize number
   * @param orderBy Record<string, boolean>
   */
  filter(userName: string | null, roleId: string | null, pageIndex: number | null, pageSize: number | null, orderBy: Record<string, boolean> | null): Observable<PageList<SysUserItemDto>> {
    const _url = `/api/SysUser?userName=${userName ?? ''}&roleId=${roleId ?? ''}&pageIndex=${pageIndex ?? ''}&pageSize=${pageSize ?? ''}&orderBy=${orderBy ?? ''}`;
    return this.request<PageList<SysUserItemDto>>('get', _url);
  }
  /**
   * 新增 ✅
   * @param data SysUserAddDto
   */
  add(data: SysUserAddDto): Observable<SysUser> {
    const _url = `/api/SysUser`;
    return this.request<SysUser>('post', _url, data);
  }
  /**
   * 更新 ✅
   * @param id
   * @param data SysUserUpdateDto
   */
  update(id: string, data: SysUserUpdateDto): Observable<SysUser> {
    const _url = `/api/SysUser/${id}`;
    return this.request<SysUser>('patch', _url, data);
  }
  /**
   * 详情 ✅
   * @param id
   */
  getDetail(id: string): Observable<SysUserDetailDto> {
    const _url = `/api/SysUser/${id}`;
    return this.request<SysUserDetailDto>('get', _url);
  }
  /**
   * ⚠删除 ✅
   * @param id
   */
  delete(id: string): Observable<void> {
    const _url = `/api/SysUser/${id}`;
    return this.request<void>('delete', _url);
  }
  /**
   * 修改密码 ✅
   * @param password string
   * @param newPassword string
   */
  changePassword(password: string | null, newPassword: string | null): Observable<boolean> {
    const _url = `/api/SysUser/changePassword?password=${password ?? ''}&newPassword=${newPassword ?? ''}`;
    return this.request<boolean>('patch', _url);
  }
}