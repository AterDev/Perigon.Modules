import { signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageEvent } from '@angular/material/paginator';
import { catchError, EMPTY, Observable, Subject, switchMap } from 'rxjs';
import { PageList } from 'src/app/services/admin/models/perigon/page-list.model';

export function createNotificationPage<T>(
  fetchPage: (page: {
    pageIndex: number;
    pageSize: number;
  }) => Observable<PageList<T>>,
) {
  const rows = signal<T[]>([]);
  const loading = signal(false);
  const error = signal(false);
  const total = signal(0);
  const pageIndex = signal(0);
  const pageSize = signal(20);
  const requests = new Subject<void>();

  requests
    .pipe(
      switchMap(() => {
        loading.set(true);
        error.set(false);
        return fetchPage({
          pageIndex: pageIndex() + 1,
          pageSize: pageSize(),
        }).pipe(
          catchError(() => {
            loading.set(false);
            error.set(true);
            return EMPTY;
          }),
        );
      }),
      takeUntilDestroyed(),
    )
    .subscribe((page) => {
      total.set(page.count);
      const lastPage = Math.max(0, Math.ceil(page.count / pageSize()) - 1);
      if (pageIndex() > lastPage) {
        pageIndex.set(lastPage);
        requests.next();
        return;
      }
      rows.set(page.data);
      loading.set(false);
    });

  return {
    rows: rows.asReadonly(),
    loading: loading.asReadonly(),
    error: error.asReadonly(),
    total: total.asReadonly(),
    pageIndex: pageIndex.asReadonly(),
    pageSize: pageSize.asReadonly(),
    load: () => requests.next(),
    onPage: (event: PageEvent) => {
      pageIndex.set(event.pageIndex);
      pageSize.set(event.pageSize);
      requests.next();
    },
  };
}

export type NotificationPage<T> = ReturnType<typeof createNotificationPage<T>>;
