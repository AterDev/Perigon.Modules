import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { AdminClient } from '../../../../services/admin/admin-client';
import { ResDefinition } from '../../../../services/admin/models/entity/res-definition.model';
import { ResValueType } from '../../../../services/admin/models/entity/res-value-type.model';
import { CommonListModules } from '../../../share/shared-modules';
import { ConfirmDialogComponent } from '../../../share/components/confirm-dialog/confirm-dialog.component';
import { I18N_KEYS } from '../../../share/i18n-keys';
import { ResourceDefinitionDialogComponent } from './definition-dialog/definition-dialog';

@Component({
  selector: 'app-resource-definition-index',
  imports: CommonListModules,
  templateUrl: './index.html',
  styleUrl: './index.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceDefinitionIndexComponent {
  readonly i18nKeys = I18N_KEYS;
  readonly definitions = signal<ResDefinition[]>([]);
  readonly valueTypeLabels = {
    [ResValueType.String]: I18N_KEYS.resource.propertyTypes.string,
    [ResValueType.Number]: I18N_KEYS.resource.propertyTypes.number,
    [ResValueType.Boolean]: I18N_KEYS.resource.propertyTypes.boolean,
    [ResValueType.Date]: I18N_KEYS.resource.propertyTypes.date,
    [ResValueType.Uri]: I18N_KEYS.resource.propertyTypes.uri,
    [ResValueType.IPAddress]: I18N_KEYS.resource.propertyTypes.ipAddress,
  };
  name = '';
  private readonly client = inject(AdminClient);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  constructor() {
    this.load();
  }

  load(): void {
    this.client.resourceConfiguration
      .definitions(this.name || null)
      .subscribe((value) => this.definitions.set(value));
  }

  createDefinition(): void {
    this.dialog
      .open(ResourceDefinitionDialogComponent, { width: '900px', maxWidth: '96vw', maxHeight: '96vh' })
      .afterClosed()
      .subscribe((value) => {
        if (!value) return;
        this.client.resourceConfiguration.addDefinition(value).subscribe(() => {
          this.showSuccess('resource.createSuccess');
          this.load();
        });
      });
  }

  editDefinition(item: ResDefinition): void {
    this.dialog
      .open(ResourceDefinitionDialogComponent, {
        width: '900px',
        maxWidth: '96vw',
        maxHeight: '96vh',
        data: { definition: item },
      })
      .afterClosed()
      .subscribe((value) => {
        if (!value) return;
        this.client.resourceConfiguration.updateDefinition(item.id, value).subscribe(() => {
          this.showSuccess('resource.updateSuccess');
          this.load();
        });
      });
  }

  deleteDefinition(item: ResDefinition): void {
    this.dialog
      .open(ConfirmDialogComponent, {
        data: {
          title: this.translate.instant('common.confirmDelete'),
          content: this.translate.instant('resource.deleteDefinitionConfirm', {
            name: item.name,
          }),
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.client.resourceConfiguration
          .deleteDefinition(item.id)
          .subscribe(() => {
            this.snackBar.open(
              this.translate.instant('resource.deleteSuccess'),
              this.translate.instant('common.close'),
              { duration: 2500 },
            );
            this.load();
          });
      });
  }

  private showSuccess(key: string): void {
    this.snackBar.open(
      this.translate.instant(key),
      this.translate.instant('common.close'),
      { duration: 2500 },
    );
  }
}
