export interface NotificationReceiverItemDto {
  id: string;
  name: string;
  type: string;
  enabled: boolean;
  hasSecret: boolean;
  endpoint: string;
  httpMethod: string;
  authType: string;
  authName: string;
  authScheme: string;
  bodyTemplate: string;
  timeoutSeconds: number;
  rateLimitPerMinute: number;
  successField: string;
  successValue: string;
}
