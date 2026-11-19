export interface NotificationRuleAddDto {
  name: string;
  sourceId: string;
  receiverIds: string[];
  eventTypes: string[];
  statuses: string[];
  projectIds: number[];
  enabled: boolean;
}
