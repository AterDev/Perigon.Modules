export interface NotificationSourceAddDto {
  name: string;
  type: string;
  enabled: boolean;
  secret: string | null;
  authType: string;
  headerKey: string;
}
