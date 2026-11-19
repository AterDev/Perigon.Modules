import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  model,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { catchError, map, of, switchMap, timer } from 'rxjs';
import { CommonFormModules } from '../../shared-modules';
import { I18N_KEYS } from '../../i18n-keys';
import { SysUserOptionDto } from 'src/app/services/admin/models/system-mod/sys-user-option-dto.model';
import { SysUserOptionsService } from 'src/app/services/admin/services/sys-user-options.service';

@Component({
  selector: 'app-user-picker',
  imports: [CommonFormModules, MatAutocompleteModule],
  templateUrl: './user-picker.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserPickerComponent {
  readonly label = input.required<string>();
  readonly disabled = input(false);
  readonly userIds = model<string[]>([]);
  readonly i18nKeys = I18N_KEYS;
  readonly search = new FormControl<string | SysUserOptionDto>('', {
    nonNullable: true,
  });
  readonly failed = signal(false);
  readonly results = signal<SysUserOptionDto[]>([]);
  readonly names = signal<Partial<Record<string, string>>>({});
  readonly available = computed(() =>
    this.results().filter((user) => !this.userIds().includes(user.id)),
  );

  private readonly users = inject(SysUserOptionsService);

  constructor() {
    this.search.valueChanges
      .pipe(
        map((value) => (typeof value === 'string' ? value.trim() : '')),
        switchMap((term) => {
          this.failed.set(false);
          this.results.set([]);
          if (term.length < 2) {
            return of([]);
          }
          return timer(250).pipe(
            switchMap(() => this.users.search(term)),
            catchError(() => {
              this.failed.set(true);
              return of([]);
            }),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((results) => this.results.set(results));
  }

  add(user: SysUserOptionDto): void {
    this.names.update((names) => ({ ...names, [user.id]: user.name }));
    if (!this.userIds().includes(user.id)) {
      this.userIds.update((ids) => [...ids, user.id]);
    }
    this.search.setValue('');
  }

  remove(id: string): void {
    this.userIds.update((ids) => ids.filter((value) => value !== id));
  }
}
