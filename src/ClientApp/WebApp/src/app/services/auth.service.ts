import { Injectable } from '@angular/core';
import { AccessTokenDto } from 'src/app/services/admin/models/share/access-token-dto.model';
import { SysUserInfoDto } from 'src/app/services/admin/models/system-mod/sys-user-info-dto.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  isLogin = false;
  isAdmin = false;
  userName?: string | null = null;
  id?: string | null = null;
  constructor() {
    this.updateUserLoginState();
  }

  saveToken(token: AccessTokenDto): void {
    this.isLogin = true;
    localStorage.setItem('accessToken', token.accessToken);
    localStorage.setItem('refreshToken', token.refreshToken);
  }

  saveUserInfo(userinfo: SysUserInfoDto): void {
    this.isLogin = true;
    this.userName = userinfo.username;
    this.id = userinfo.id;
    this.isAdmin = userinfo.roles.some(
      (role) => role === 'AdminUser' || role === 'SuperAdmin',
    );
    localStorage.setItem('username', userinfo.username);
    localStorage.setItem('userId', userinfo.id);
    localStorage.setItem('isAdmin', String(this.isAdmin));
  }

  updateUserLoginState(): void {
    const username = localStorage.getItem('username');
    const token = localStorage.getItem('accessToken');
    if (token && username) {
      this.userName = username;
      this.id = localStorage.getItem('userId');
      this.isAdmin = localStorage.getItem('isAdmin') === 'true';
      this.isLogin = true;
    } else {
      this.isLogin = false;
    }
  }
  logout(): void {
    localStorage.clear();
    this.isLogin = false;
    this.isAdmin = false;
    this.id = null;
  }
}
