import { InAppNotificationItemDto } from './in-app-notification-item-dto.model';

export interface InAppNotificationDetailDto extends InAppNotificationItemDto {
  updatedTime: string;
  targetUserIds: string[];
}
