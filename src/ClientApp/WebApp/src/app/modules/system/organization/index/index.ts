import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { ConfirmDialogComponent } from 'src/app/modules/share/components/confirm-dialog/confirm-dialog.component';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { CommonListModules } from 'src/app/modules/share/shared-modules';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { SysOrganization } from 'src/app/services/admin/models/entity/sys-organization.model';
import {
  SysOrganizationFormComponent,
  SysOrganizationOption,
} from 'src/app/modules/system/organization/form/form';

interface FlattenedOrganization {
  organization: SysOrganization;
  level: number;
  hasChildren: boolean;
}

@Component({
  selector: 'app-sys-organization-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SysOrganizationIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  readonly organizations = signal<SysOrganization[]>([]);
  readonly loading = signal(false);
  readonly loadFailed = signal(false);
  private readonly expandedIds = signal<Set<string>>(new Set());
  readonly rows = computed(() => this.flatten(this.organizations()));

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.client.sysOrganization.list().subscribe({
      next: (organizations) => {
        this.organizations.set(organizations ?? []);
        this.expandedIds.set(new Set(this.collectIds(organizations ?? [])));
        this.loading.set(false);
      },
      error: () => {
        this.loadFailed.set(true);
        this.loading.set(false);
      },
    });
  }

  toggle(organization: SysOrganization): void {
    if (!organization.children?.length) {
      return;
    }
    const expanded = new Set(this.expandedIds());
    if (expanded.has(organization.id)) {
      expanded.delete(organization.id);
    } else {
      expanded.add(organization.id);
    }
    this.expandedIds.set(expanded);
  }

  isExpanded(organization: SysOrganization): boolean {
    return this.expandedIds().has(organization.id);
  }

  add(parentId: string | null = null): void {
    this.dialog
      .open(SysOrganizationFormComponent, {
        width: '520px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { parentId, options: this.options() },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) {
          this.load();
        }
      });
  }

  edit(organization: SysOrganization): void {
    this.dialog
      .open(SysOrganizationFormComponent, {
        width: '520px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: {
          organization,
          options: this.options(organization.id),
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.saved) {
          this.load();
        }
      });
  }

  remove(organization: SysOrganization): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant(this.i18nKeys.common.confirmDelete),
          content: this.translate.instant(
            this.i18nKeys.systemOrganization.deleteConfirm,
            { name: organization.name },
          ),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.client.sysOrganization.delete(organization.id).subscribe({
          next: () => {
            this.snackBar.open(
              this.translate.instant(
                this.i18nKeys.systemOrganization.deleteSuccess,
              ),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 2500 },
            );
            this.load();
          },
          error: () => {
            this.snackBar.open(
              this.translate.instant(this.i18nKeys.common.deleteFail),
              this.translate.instant(this.i18nKeys.common.close),
              { duration: 3000 },
            );
          },
        });
      });
  }

  options(excludeId?: string): SysOrganizationOption[] {
    const result: SysOrganizationOption[] = [];
    this.collectOptions(this.organizations(), 0, result, excludeId);
    return result;
  }

  private flatten(
    organizations: SysOrganization[],
    level = 0,
    rows: FlattenedOrganization[] = [],
  ): FlattenedOrganization[] {
    for (const organization of organizations) {
      rows.push({
        organization,
        level,
        hasChildren: !!organization.children?.length,
      });
      if (
        organization.children?.length &&
        this.expandedIds().has(organization.id)
      ) {
        this.flatten(organization.children, level + 1, rows);
      }
    }
    return rows;
  }

  private collectIds(
    organizations: SysOrganization[],
    ids: string[] = [],
  ): string[] {
    for (const organization of organizations) {
      ids.push(organization.id);
      if (organization.children?.length) {
        this.collectIds(organization.children, ids);
      }
    }
    return ids;
  }

  private collectOptions(
    organizations: SysOrganization[],
    level: number,
    result: SysOrganizationOption[],
    excludeId?: string,
  ): void {
    for (const organization of organizations) {
      if (organization.id === excludeId) {
        continue;
      }
      result.push({ organization, level });
      if (organization.children?.length) {
        this.collectOptions(organization.children, level + 1, result, excludeId);
      }
    }
  }
}
