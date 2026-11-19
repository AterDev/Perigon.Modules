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
import { NotificationSourceAddDto } from 'src/app/services/admin/models/notification-mod/notification-source-add-dto.model';
import { NotificationSourceItemDto } from 'src/app/services/admin/models/notification-mod/notification-source-item-dto.model';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { NotificationActions } from '../shared/notification-actions';
import { NotificationListComponent } from '../shared/notification-list';
import { createNotificationPage } from '../shared/notification-page';

@Component({
  selector: 'app-notification-sources',
  imports: [
    ...CommonListModules,
    ReactiveFormsModule,
    NotificationListComponent,
  ],
  templateUrl: './sources.html',
  styleUrl: '../shared/notification-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationSourcesComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly notificationKeys = I18N_KEYS.notification;
  readonly editor = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly hasSavedSecret = signal(false);
  readonly list = createNotificationPage((page) =>
    this.client.notification.sources(page),
  );
  readonly form = inject(FormBuilder).nonNullable.group({
    sourceName: ['', [Validators.required, Validators.maxLength(100)]],
    sourceType: ['gitlab', Validators.required],
    authType: ['query', Validators.required],
    headerKey: ['X-Gitlab-Token', Validators.maxLength(100)],
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
      sourceName: '',
      sourceType: 'gitlab',
      authType: 'query',
      headerKey: 'X-Gitlab-Token',
      secret: '',
      enabled: true,
    });
    this.editor.set(true);
  }

  editSource(item: NotificationSourceItemDto): void {
    this.editingId.set(item.id);
    this.hasSavedSecret.set(item.hasSecret);
    this.form.reset({
      sourceName: item.name,
      sourceType: item.type,
      authType: item.authType,
      headerKey: item.headerKey,
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
    if (this.form.invalid || (!this.hasSavedSecret() && !value.secret.trim())) {
      this.actions.notify(this.i18nKeys.common.formInvalid);
      return;
    }

    const payload: NotificationSourceAddDto = {
      name: value.sourceName,
      type: value.sourceType,
      enabled: value.enabled,
      secret: value.secret || null,
      authType: value.authType,
      headerKey: value.headerKey,
    };
    const id = this.editingId();
    this.saving.set(true);
    const request = id
      ? this.client.notification.updateSource(id, payload)
      : this.client.notification.addSource(payload);
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

  remove(item: NotificationSourceItemDto): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.actions
      .confirmDelete()
      .pipe(
        switchMap(() => this.client.notification.deleteSource(item.id)),
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
