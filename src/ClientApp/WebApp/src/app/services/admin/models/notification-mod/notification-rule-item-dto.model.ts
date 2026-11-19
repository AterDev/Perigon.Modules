export interface NotificationRuleItemDto {
  id: string;
  name: string;
  sourceId: string;
  receiverIds: string[];
  eventTypes: string[];
  statuses: string[];
  projectIds: number[];
  enabled: boolean;
}
