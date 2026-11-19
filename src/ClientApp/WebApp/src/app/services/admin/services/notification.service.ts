import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseService } from '../base.service';
import { PageList } from '../models/perigon/page-list.model';
import { InAppNotificationAddDto } from '../models/notification-mod/in-app-notification-add-dto.model';
import { InAppNotificationDetailDto } from '../models/notification-mod/in-app-notification-detail-dto.model';
import { InAppNotificationItemDto } from '../models/notification-mod/in-app-notification-item-dto.model';
import { InAppNotificationUpdateDto } from '../models/notification-mod/in-app-notification-update-dto.model';
import { NotificationSourceAddDto } from '../models/notification-mod/notification-source-add-dto.model';
import { NotificationSourceItemDto } from '../models/notification-mod/notification-source-item-dto.model';
import { NotificationReceiverAddDto } from '../models/notification-mod/notification-receiver-add-dto.model';
import { NotificationReceiverItemDto } from '../models/notification-mod/notification-receiver-item-dto.model';
import { NotificationRuleAddDto } from '../models/notification-mod/notification-rule-add-dto.model';
import { NotificationRuleItemDto } from '../models/notification-mod/notification-rule-item-dto.model';
import { NotificationEventItemDto } from '../models/notification-mod/notification-event-item-dto.model';
import { NotificationDeliveryItemDto } from '../models/notification-mod/notification-delivery-item-dto.model';
import { InAppNotificationReadDto } from '../models/notification-mod/in-app-notification-read-dto.model';
import { NotificationRecoveryDto } from '../models/notification-mod/notification-recovery-dto.model';
import { NotificationRecoveryResultDto } from '../models/notification-mod/notification-recovery-result-dto.model';

interface PageFilter {
  pageIndex: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationService extends BaseService {
  list(
    filter: {
      title?: string;
      type?: string;
      published?: boolean;
    } & Partial<PageFilter>,
  ): Observable<PageList<InAppNotificationItemDto>> {
    return this.request<PageList<InAppNotificationItemDto>>(
      'post',
      '/api/Notification/filter',
      this.filter(filter),
    );
  }

  detail(id: string): Observable<InAppNotificationDetailDto> {
    return this.request<InAppNotificationDetailDto>(
      'get',
      `/api/Notification/${id}`,
    );
  }

  add(data: InAppNotificationAddDto): Observable<InAppNotificationDetailDto> {
    return this.request<InAppNotificationDetailDto>(
      'post',
      '/api/Notification',
      data,
    );
  }

  update(id: string, data: InAppNotificationUpdateDto): Observable<boolean> {
    return this.request<boolean>('patch', `/api/Notification/${id}`, data);
  }

  publish(id: string): Observable<void> {
    return this.request<void>('post', `/api/Notification/${id}/publish`);
  }

  delete(id: string): Observable<void> {
    return this.request<void>('delete', `/api/Notification/${id}`);
  }

  mine(
    filter: {
      title?: string;
      type?: string;
      published?: boolean;
    } & Partial<PageFilter> = {},
  ): Observable<PageList<InAppNotificationItemDto>> {
    return this.request<PageList<InAppNotificationItemDto>>(
      'post',
      '/api/Notification/mine',
      this.filter(filter),
    );
  }

  mineDetail(id: string): Observable<InAppNotificationDetailDto> {
    return this.request<InAppNotificationDetailDto>(
      'get',
      `/api/Notification/mine/${id}`,
    );
  }

  unreadCount(): Observable<number> {
    return this.request<number>('get', '/api/Notification/mine/unread-count');
  }

  read(id: string): Observable<InAppNotificationReadDto> {
    return this.request<InAppNotificationReadDto>(
      'post',
      `/api/Notification/${id}/read`,
    );
  }

  sources(
    filter: Partial<PageFilter> = {},
  ): Observable<PageList<NotificationSourceItemDto>> {
    return this.request<PageList<NotificationSourceItemDto>>(
      'post',
      '/api/Notification/sources/filter',
      this.filter(filter),
    );
  }

  addSource(
    data: NotificationSourceAddDto,
  ): Observable<NotificationSourceItemDto> {
    return this.request<NotificationSourceItemDto>(
      'post',
      '/api/Notification/sources',
      data,
    );
  }

  updateSource(
    id: string,
    data: NotificationSourceAddDto,
  ): Observable<NotificationSourceItemDto> {
    return this.request<NotificationSourceItemDto>(
      'patch',
      `/api/Notification/sources/${id}`,
      data,
    );
  }

  deleteSource(id: string): Observable<void> {
    return this.request<void>('delete', `/api/Notification/sources/${id}`);
  }

  receivers(
    filter: Partial<PageFilter> = {},
  ): Observable<PageList<NotificationReceiverItemDto>> {
    return this.request<PageList<NotificationReceiverItemDto>>(
      'post',
      '/api/Notification/receivers/filter',
      this.filter(filter),
    );
  }

  addReceiver(
    data: NotificationReceiverAddDto,
  ): Observable<NotificationReceiverItemDto> {
    return this.request<NotificationReceiverItemDto>(
      'post',
      '/api/Notification/receivers',
      data,
    );
  }

  updateReceiver(
    id: string,
    data: NotificationReceiverAddDto,
  ): Observable<NotificationReceiverItemDto> {
    return this.request<NotificationReceiverItemDto>(
      'patch',
      `/api/Notification/receivers/${id}`,
      data,
    );
  }

  deleteReceiver(id: string): Observable<void> {
    return this.request<void>('delete', `/api/Notification/receivers/${id}`);
  }

  rules(
    filter: Partial<PageFilter> = {},
  ): Observable<PageList<NotificationRuleItemDto>> {
    return this.request<PageList<NotificationRuleItemDto>>(
      'post',
      '/api/Notification/rules/filter',
      this.filter(filter),
    );
  }

  addRule(data: NotificationRuleAddDto): Observable<NotificationRuleItemDto> {
    return this.request<NotificationRuleItemDto>(
      'post',
      '/api/Notification/rules',
      data,
    );
  }

  updateRule(
    id: string,
    data: NotificationRuleAddDto,
  ): Observable<NotificationRuleItemDto> {
    return this.request<NotificationRuleItemDto>(
      'patch',
      `/api/Notification/rules/${id}`,
      data,
    );
  }

  deleteRule(id: string): Observable<void> {
    return this.request<void>('delete', `/api/Notification/rules/${id}`);
  }

  events(
    filter: Partial<PageFilter> = {},
  ): Observable<PageList<NotificationEventItemDto>> {
    return this.request<PageList<NotificationEventItemDto>>(
      'post',
      '/api/Notification/events/filter',
      this.filter(filter),
    );
  }

  deliveries(
    filter: Partial<PageFilter> = {},
  ): Observable<PageList<NotificationDeliveryItemDto>> {
    return this.request<PageList<NotificationDeliveryItemDto>>(
      'post',
      '/api/Notification/deliveries/filter',
      this.filter(filter),
    );
  }

  recover(
    data: NotificationRecoveryDto = { leaseSeconds: 60 },
  ): Observable<NotificationRecoveryResultDto> {
    return this.request<NotificationRecoveryResultDto>(
      'post',
      '/api/Notification/recovery',
      data,
    );
  }

  private filter(
    value: Partial<PageFilter> & Record<string, unknown>,
  ): PageFilter & Record<string, unknown> {
    return { pageIndex: 1, pageSize: 20, ...value };
  }
}
