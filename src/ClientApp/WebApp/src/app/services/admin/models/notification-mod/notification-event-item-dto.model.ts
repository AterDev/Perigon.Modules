export interface NotificationEventItemDto {
  id: string;
  eventType: string;
  title: string;
  content: string;
  status: string;
  url: string;
  occurredAt: string | null;
  processingStatus: string;
}
