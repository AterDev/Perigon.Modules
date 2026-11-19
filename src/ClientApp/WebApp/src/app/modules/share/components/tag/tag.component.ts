import { CommonModule } from '@angular/common';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-tag',
  imports: [CommonModule],
  templateUrl: './tag.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './tag.component.scss',
})
export class TagComponent {
  @Input() color: string = 'primary';
  constructor() {}
}
