import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, Observable, Subscription, switchMap } from 'rxjs';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { UserPickerComponent } from 'src/app/modules/share/components/user-picker/user-picker';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { InAppNotificationItemDto } from 'src/app/services/admin/models/notification-mod/in-app-notification-item-dto.model';
import { NotificationActions } from '../shared/notification-actions';
import { NotificationListComponent } from '../shared/notification-list';
import { createNotificationPage } from '../shared/notification-page';

@Component({
  selector: 'app-notification-messages',
  imports: [
    ...CommonListModules,
    ReactiveFormsModule,
    UserPickerComponent,
    NotificationListComponent,
  ],
  templateUrl: './messages.html',
  styleUrl: '../shared/notification-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationMessagesComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly notificationKeys = I18N_KEYS.notification;
  readonly list = createNotificationPage((page) =>
    this.client.notification.list(page),
  );
  readonly editor = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly detailLoading = signal(false);
  readonly saving = signal(false);

  readonly form = inject(FormBuilder).nonNullable.group({
    type: ['message', Validators.required],
    title: ['', [Validators.required, Validators.maxLength(200)]],
    content: ['', [Validators.required, Validators.maxLength(20000)]],
    link: ['', Validators.maxLength(1200)],
    expiresTime: [''],
    targetUserIds: [[] as string[]],
  });

  private readonly client = inject(AdminClient);
  private readonly actions = inject(NotificationActions);
  private readonly destroyRef = inject(DestroyRef);
  private detailRequest?: Subscription;

  constructor() {
    this.list.load();
  }

  startAdd(): void {
    this.detailRequest?.unsubscribe();
    this.detailLoading.set(false);
    this.editingId.set(null);
    this.editor.set(true);
    this.form.reset();
  }

  editNotification(item: InAppNotificationItemDto): void {
    if (item.isPublished || this.saving()) {
      return;
    }

    this.detailRequest?.unsubscribe();
    this.editor.set(true);
    this.editingId.set(item.id);
    this.detailLoading.set(true);
    this.form.reset({ type: item.type });

    this.detailRequest = this.client.notification
      .detail(item.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.detailLoading.set(false)),
      )
      .subscribe({
        next: (detail) => {
          this.form.patchValue({
            type: detail.type,
            title: detail.title,
            content: detail.content,
            link: detail.link,
            expiresTime: this.toLocalDateTime(detail.expiresTime),
            targetUserIds: [...detail.targetUserIds],
          });
        },
        error: () => {
          this.cancelEdit();
          this.actions.notify(this.i18nKeys.common.loadFail);
        },
      });
  }

  cancelEdit(): void {
    if (this.saving()) {
      return;
    }

    this.detailRequest?.unsubscribe();
    this.detailRequest = undefined;
    this.editor.set(false);
    this.editingId.set(null);
    this.detailLoading.set(false);
    this.form.reset();
  }

  save(): void {
    if (this.saving() || this.detailLoading()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.actions.notify(this.i18nKeys.common.formInvalid);
      return;
    }

    const value = this.form.getRawValue();
    const payload = {
      ...value,
      expiresTime: value.expiresTime
        ? new Date(value.expiresTime).toISOString()
        : null,
    };
    const id = this.editingId();
    this.saving.set(true);

    const request$: Observable<unknown> = id
      ? this.client.notification.update(id, {
          ...payload,
          clearExpiresTime: !payload.expiresTime,
        })
      : this.client.notification.add({
          ...payload,
          publish: false,
        });

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.saving.set(false)),
      )
      .subscribe({
        next: () => {
          this.actions.notify(
            id
              ? this.notificationKeys.updateSuccess
              : this.notificationKeys.createSuccess,
          );
          this.saving.set(false);
          this.cancelEdit();
          this.list.load();
        },
        error: () => this.actions.notify(this.i18nKeys.common.saveFail),
      });
  }

  publish(item: InAppNotificationItemDto): void {
    if (item.isPublished || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.client.notification
      .detail(item.id)
      .pipe(
        switchMap((detail) =>
          this.actions.confirm(
            this.notificationKeys.publish,
            detail.targetUserIds.length > 0
              ? this.notificationKeys.publishTargeted
              : this.notificationKeys.publishAll,
            { title: detail.title, count: detail.targetUserIds.length },
          ),
        ),
        switchMap(() => this.client.notification.publish(item.id)),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.saving.set(false)),
      )
      .subscribe({
        next: () => {
          this.actions.notify(this.notificationKeys.publishSuccess);
          this.list.load();
        },
        error: () => this.actions.notify(this.i18nKeys.common.saveFail),
      });
  }

  remove(item: InAppNotificationItemDto): void {
    if (item.isPublished || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.actions
      .confirmDelete()
      .pipe(
        switchMap(() => this.client.notification.delete(item.id)),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.saving.set(false)),
      )
      .subscribe({
        next: () => {
          this.actions.notify(this.notificationKeys.deleteSuccess);
          this.list.load();
        },
        error: () => this.actions.notify(this.i18nKeys.common.deleteFail),
      });
  }

  private toLocalDateTime(value: string | null): string {
    if (!value) {
      return '';
    }
    const date = new Date(value);
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
      .toISOString()
      .slice(0, 16);
  }
}
