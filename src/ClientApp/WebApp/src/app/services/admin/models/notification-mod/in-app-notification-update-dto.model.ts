export interface InAppNotificationUpdateDto {
  type?: string;
  title?: string;
  content?: string;
  link?: string;
  expiresTime?: string | null;
  clearExpiresTime?: boolean;
  targetUserIds?: string[];
}
