import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

/// <summary>限制管理员专用页面的前端导航入口；后端接口仍负责最终授权。</summary>
@Injectable({ providedIn: 'root' })
export class AdminGuard implements CanActivate {
  constructor(
    private readonly router: Router,
    private readonly auth: AuthService,
  ) {}

  canActivate(
    _route: ActivatedRouteSnapshot,
    _state: RouterStateSnapshot,
  ): boolean | UrlTree {
    return this.auth.isAdmin ? true : this.router.parseUrl('/resource/mine');
  }
}
