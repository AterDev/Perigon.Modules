import { SlicePipe } from '@angular/common';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-avatar',
  imports: [SlicePipe],
  templateUrl: './avatar.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./avatar.component.css'],
})
export class AvatarComponent {
  @Input()
  avatar: string | null | undefined = null;
  @Input()
  name: string | null | undefined = null;
  @Input()
  size: number = 20;
  constructor() {}
}
