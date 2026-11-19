import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import { EMPTY, Observable, Subscription, forkJoin } from 'rxjs';
import { expand, finalize, reduce, switchMap } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { NotificationReceiverItemDto } from 'src/app/services/admin/models/notification-mod/notification-receiver-item-dto.model';
import { NotificationRuleAddDto } from 'src/app/services/admin/models/notification-mod/notification-rule-add-dto.model';
import { NotificationRuleItemDto } from 'src/app/services/admin/models/notification-mod/notification-rule-item-dto.model';
import { NotificationSourceItemDto } from 'src/app/services/admin/models/notification-mod/notification-source-item-dto.model';
import { PageList } from 'src/app/services/admin/models/perigon/page-list.model';
import { NotificationActions } from '../shared/notification-actions';
import { NotificationListComponent } from '../shared/notification-list';
import { createNotificationPage } from '../shared/notification-page';

@Component({
  selector: 'app-notification-rules',
  imports: [
    NotificationListComponent,
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    TranslateModule,
  ],
  templateUrl: './rules.html',
  styleUrl: '../shared/notification-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationRulesComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly notificationKeys = I18N_KEYS.notification;
  private readonly formBuilder = inject(NonNullableFormBuilder);
  readonly list = createNotificationPage((page) =>
    this.client.notification.rules(page),
  );
  readonly editorOpen = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly ruleOptionsLoading = signal(false);
  readonly ruleOptionsError = signal(false);
  readonly sourceOptions = signal<NotificationSourceItemDto[]>([]);
  readonly receiverOptions = signal<NotificationReceiverItemDto[]>([]);
  readonly form = this.formBuilder.group({
    ruleName: ['', Validators.required],
    sourceId: ['', Validators.required],
    receiverIds: this.formBuilder.control<string[]>([], Validators.required),
    eventTypes: this.formBuilder.control<string[]>([], Validators.required),
    statuses: ['success,failed'],
    projectIds: [''],
    enabled: [true],
  });

  private readonly client = inject(AdminClient);
  private readonly actions = inject(NotificationActions);
  private readonly destroyRef = inject(DestroyRef);
  private ruleOptionsRequest?: Subscription;

  constructor() {
    this.list.load();
  }

  startAdd(): void {
    this.editingId.set(null);
    this.editorOpen.set(true);
    this.form.reset({
      ruleName: '',
      sourceId: '',
      receiverIds: [],
      eventTypes: [],
      statuses: 'success,failed',
      projectIds: '',
      enabled: true,
    });
    this.loadRuleOptions();
  }

  editRule(item: NotificationRuleItemDto): void {
    this.editingId.set(item.id);
    this.editorOpen.set(true);
    this.form.reset({
      ruleName: item.name,
      sourceId: item.sourceId,
      receiverIds: item.receiverIds,
      eventTypes: item.eventTypes,
      statuses: item.statuses.join(','),
      projectIds: item.projectIds.join(','),
      enabled: item.enabled,
    });
    this.loadRuleOptions();
  }

  cancelEdit(): void {
    this.ruleOptionsRequest?.unsubscribe();
    this.ruleOptionsRequest = undefined;
    this.editorOpen.set(false);
    this.editingId.set(null);
  }

  save(): void {
    if (this.saving() || !this.editorOpen()) {
      return;
    }
    if (!this.isEditorValid()) {
      this.actions.notify(this.i18nKeys.common.formInvalid);
      return;
    }

    const value = this.form.getRawValue();
    const payload: NotificationRuleAddDto = {
      name: value.ruleName.trim(),
      sourceId: value.sourceId,
      receiverIds: this.csv(value.receiverIds),
      eventTypes: this.csv(value.eventTypes),
      statuses: this.csv(value.statuses),
      projectIds: this.csv(value.projectIds).map(Number),
      enabled: value.enabled,
    };
    const id = this.editingId();
    const request = id
      ? this.client.notification.updateRule(id, payload)
      : this.client.notification.addRule(payload);

    this.saving.set(true);
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

  remove(item: NotificationRuleItemDto): void {
    if (this.saving()) return;
    this.saving.set(true);
    this.actions
      .confirmDelete()
      .pipe(
        switchMap(() => this.client.notification.deleteRule(item.id)),
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

  sourceChanged(): void {
    this.form.controls.eventTypes.reset([]);
  }

  ruleEventTypes(): string[] {
    const source = this.sourceOptions().find(
      (item) => item.id === this.form.controls.sourceId.value,
    );
    return source?.type === 'gitlab'
      ? ['pipeline', 'push', 'tag_push', 'merge_request']
      : source?.type === 'event'
        ? ['event']
        : source?.type === 'notification'
          ? ['notification']
          : [];
  }

  loadRuleOptions(): void {
    this.ruleOptionsRequest?.unsubscribe();
    this.ruleOptionsLoading.set(true);
    this.ruleOptionsError.set(false);
    const service = this.client.notification;
    this.ruleOptionsRequest = forkJoin({
      sources: this.loadAllOptions((pageIndex) =>
        service.sources({ pageIndex, pageSize: 100 }),
      ),
      receivers: this.loadAllOptions((pageIndex) =>
        service.receivers({ pageIndex, pageSize: 100 }),
      ),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ sources, receivers }) => {
          this.sourceOptions.set(sources);
          this.receiverOptions.set(receivers);
          this.ruleOptionsLoading.set(false);
        },
        error: () => {
          this.ruleOptionsLoading.set(false);
          this.ruleOptionsError.set(true);
        },
      });
  }

  private loadAllOptions<T>(
    load: (pageIndex: number) => Observable<PageList<T>>,
  ): Observable<T[]> {
    const pageSize = 100;
    return load(1).pipe(
      expand((page) =>
        page.data.length > 0 && page.pageIndex * pageSize < page.count
          ? load(page.pageIndex + 1)
          : EMPTY,
      ),
      reduce((options, page) => [...options, ...page.data], [] as T[]),
    );
  }

  private isEditorValid(): boolean {
    this.form.markAllAsTouched();
    return (
      this.form.valid &&
      this.csv(this.form.controls.receiverIds.value).length > 0 &&
      this.csv(this.form.controls.eventTypes.value).length > 0
    );
  }

  private csv(value: string | string[]): string[] {
    return Array.isArray(value)
      ? value.filter(Boolean)
      : value
          .split(',')
          .map((item) => item.trim())
          .filter(Boolean);
  }
}
