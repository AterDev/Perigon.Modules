import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatDialogModule } from '@angular/material/dialog';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

export const BaseMatModules = [
  TranslateModule,
  CommonModule,
  MatIconModule,
  MatTooltipModule,
  MatButtonModule,
  MatProgressSpinnerModule,
  MatToolbarModule,
  MatCardModule,
];
// 表单页面依赖的模块
export const CommonFormModules = [
  ...BaseMatModules,
  MatFormFieldModule,
  MatDialogModule,
  ReactiveFormsModule,
  FormsModule,
  MatInputModule,
  MatSelectModule,
  MatDatepickerModule,
  MatCheckboxModule,
  MatExpansionModule,
  MatChipsModule,
  RouterModule,
];
// 列表页面依赖的模块
export const CommonListModules = [
  ...BaseMatModules,
  MatTableModule,
  MatPaginatorModule,
  MatDialogModule,
  RouterModule,
  MatTabsModule,
  MatExpansionModule,
  MatChipsModule,
  MatCheckboxModule,
  FormsModule,
  MatFormFieldModule,
  MatInputModule,
  MatSelectModule,
];
export const CommonModules = [CommonModule, RouterModule];
