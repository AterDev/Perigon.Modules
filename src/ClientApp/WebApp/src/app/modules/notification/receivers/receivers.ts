import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, switchMap } from 'rxjs';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { NotificationReceiverAddDto } from 'src/app/services/admin/models/notification-mod/notification-receiver-add-dto.model';
import { NotificationReceiverItemDto } from 'src/app/services/admin/models/notification-mod/notification-receiver-item-dto.model';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { NotificationActions } from '../shared/notification-actions';
import { NotificationListComponent } from '../shared/notification-list';
import { createNotificationPage } from '../shared/notification-page';

@Component({
  selector: 'app-notification-receivers',
  imports: [
    ...CommonListModules,
    ReactiveFormsModule,
    NotificationListComponent,
  ],
  templateUrl: './receivers.html',
  styleUrl: '../shared/notification-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationReceiversComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly notificationKeys = I18N_KEYS.notification;
  readonly editor = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly hasSavedSecret = signal(false);
  readonly list = createNotificationPage((page) =>
    this.client.notification.receivers(page),
  );
  readonly form = inject(FormBuilder).nonNullable.group({
    receiverName: ['', [Validators.required, Validators.maxLength(100)]],
    receiverType: ['webhook'],
    endpoint: ['', [Validators.required, Validators.maxLength(2000)]],
    httpMethod: ['POST', Validators.required],
    authType: ['none', Validators.required],
    authName: ['key', Validators.maxLength(100)],
    authScheme: ['', Validators.maxLength(100)],
    bodyTemplate: [
      '{"content":"{content}"}',
      [Validators.required, Validators.maxLength(8000)],
    ],
    timeoutSeconds: [
      3,
      [Validators.required, Validators.min(1), Validators.max(30)],
    ],
    rateLimitPerMinute: [
      20,
      [Validators.required, Validators.min(1), Validators.max(600)],
    ],
    successField: ['', Validators.maxLength(100)],
    successValue: ['', Validators.maxLength(200)],
    secret: [''],
    enabled: [true],
  });

  private readonly client = inject(AdminClient);
  private readonly actions = inject(NotificationActions);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.list.load();
  }

  startAdd(): void {
    this.editingId.set(null);
    this.hasSavedSecret.set(false);
    this.form.reset({
      receiverName: '',
      receiverType: 'webhook',
      endpoint: '',
      httpMethod: 'POST',
      authType: 'none',
      authName: 'key',
      authScheme: '',
      bodyTemplate: '{"content":"{content}"}',
      timeoutSeconds: 3,
      rateLimitPerMinute: 20,
      successField: '',
      successValue: '',
      secret: '',
      enabled: true,
    });
    this.editor.set(true);
  }

  editReceiver(item: NotificationReceiverItemDto): void {
    this.editingId.set(item.id);
    this.hasSavedSecret.set(item.hasSecret);
    this.form.reset({
      receiverName: item.name,
      receiverType: item.type,
      endpoint: item.endpoint,
      httpMethod: item.httpMethod,
      authType: item.authType,
      authName: item.authName,
      authScheme: item.authScheme,
      bodyTemplate: item.bodyTemplate,
      timeoutSeconds: item.timeoutSeconds,
      rateLimitPerMinute: item.rateLimitPerMinute,
      successField: item.successField,
      successValue: item.successValue,
      secret: '',
      enabled: item.enabled,
    });
    this.editor.set(true);
  }

  cancelEdit(): void {
    if (this.saving()) {
      return;
    }
    this.editor.set(false);
    this.editingId.set(null);
    this.hasSavedSecret.set(false);
  }

  save(): void {
    if (this.saving()) {
      return;
    }
    const value = this.form.getRawValue();
    this.form.markAllAsTouched();
    const requiresSecret = value.authType !== 'none';
    if (
      this.form.invalid ||
      (requiresSecret && !this.hasSavedSecret() && !value.secret.trim())
    ) {
      this.actions.notify(this.i18nKeys.common.formInvalid);
      return;
    }

    const payload: NotificationReceiverAddDto = {
      name: value.receiverName,
      type: value.receiverType,
      enabled: value.enabled,
      secret: requiresSecret ? value.secret || null : null,
      endpoint: value.endpoint,
      httpMethod: value.httpMethod,
      authType: value.authType,
      authName: value.authName,
      authScheme: value.authScheme,
      bodyTemplate: value.bodyTemplate,
      timeoutSeconds: value.timeoutSeconds,
      rateLimitPerMinute: value.rateLimitPerMinute,
      successField: value.successField,
      successValue: value.successValue,
    };
    const id = this.editingId();
    this.saving.set(true);
    const request = id
      ? this.client.notification.updateReceiver(id, payload)
      : this.client.notification.addReceiver(payload);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.actions.notify(
          id
            ? this.notificationKeys.updateSuccess
            : this.notificationKeys.createSuccess,
        );
        this.cancelEdit();
        this.list.load();
      },
      error: () => {
        this.saving.set(false);
        this.actions.notify(this.i18nKeys.common.saveFail);
      },
    });
  }

  remove(item: NotificationReceiverItemDto): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.actions
      .confirmDelete()
      .pipe(
        switchMap(() => this.client.notification.deleteReceiver(item.id)),
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
}
