export interface NotificationSourceItemDto {
  id: string;
  name: string;
  type: string;
  enabled: boolean;
  hasSecret: boolean;
  authType: string;
  headerKey: string;
}
