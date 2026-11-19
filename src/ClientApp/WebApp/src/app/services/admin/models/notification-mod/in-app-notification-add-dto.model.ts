export interface InAppNotificationAddDto {
  type: string;
  title: string;
  content: string;
  link: string;
  expiresTime: string | null;
  targetUserIds: string[];
  publish: boolean;
}
