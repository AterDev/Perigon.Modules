/**
 * 登录
 */
export interface SysLoginDto {
  /** email */
  email: string;
  /** password */
  password: string;
  /** 验证码 */
  verifyCode?: string | null;
}
