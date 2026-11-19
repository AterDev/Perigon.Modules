export interface InAppNotificationItemDto {
  id: string;
  type: string;
  title: string;
  content: string;
  link: string;
  isPublished: boolean;
  publishedTime: string | null;
  expiresTime: string | null;
  isRead: boolean;
  createdTime: string;
}
