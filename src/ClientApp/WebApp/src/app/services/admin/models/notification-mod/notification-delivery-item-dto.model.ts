export interface NotificationDeliveryItemDto {
  id: string;
  eventId: string;
  eventType: string;
  projectName: string;
  objectId: string;
  receiverName: string;
  status: string;
  error: string | null;
  sentAt: string | null;
}
