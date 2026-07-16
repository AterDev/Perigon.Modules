import { AfterViewInit, Component, DestroyRef, inject } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AuthService } from 'src/app/services/auth.service';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { initStarfield } from './starfield';
import { AdminClient } from 'src/app/services/admin/admin-client';

@Component({
  selector: 'app-login',
  imports: [CommonFormModules, MatCardModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss'],
})
export class Login implements AfterViewInit {
  i18nKeys = I18N_KEYS;
  private adminClient = inject(AdminClient);
  private destroyRef = inject(DestroyRef);
  constructor(
    private authService: AuthService,
    private router: Router,
  ) {
    if (authService.isLogin) {
      this.router.navigate(['/system']);
    }
  }

  readonly loginForm = new FormGroup({
    email: new FormControl('', [
      Validators.required,
      Validators.email,
      Validators.minLength(4),
      Validators.maxLength(100),
    ]),
    password: new FormControl('', [
      Validators.required,
      Validators.minLength(6),
      Validators.maxLength(60),
    ]),
  });

  get email() {
    return this.loginForm.controls.email;
  }
  get password() {
    return this.loginForm.controls.password;
  }

  ngAfterViewInit(): void {
    const canvas = document.getElementById(
      'starfield',
    ) as HTMLCanvasElement | null;
    if (canvas) {
      const stopStarfield = initStarfield(canvas);
      this.destroyRef.onDestroy(stopStarfield);
    }
  }

  doLogin(): void {
    if (this.loginForm.invalid) return;
    const data = this.loginForm.getRawValue();
    this.adminClient.systemUser
      .login({ email: data.email ?? '', password: data.password ?? '' })
      .subscribe((res) => {
        this.authService.saveToken(res);
        this.getUserInfo();
      });
  }

  getUserInfo(): void {
    this.adminClient.systemUser.getUserInfo().subscribe((res) => {
      this.authService.saveUserInfo(res);
      this.router.navigate(['/system']);
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
