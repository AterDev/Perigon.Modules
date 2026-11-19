export interface NotificationReceiverAddDto {
  name: string;
  type: string;
  enabled: boolean;
  secret: string | null;
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
