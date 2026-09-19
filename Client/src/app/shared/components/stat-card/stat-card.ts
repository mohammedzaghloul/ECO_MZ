import { Component, Input } from '@angular/core';

export type StatTone = 'primary' | 'success' | 'warning' | 'danger';

@Component({
  selector: 'app-stat-card',
  standalone: false,
  templateUrl: './stat-card.html',
  styleUrl: './stat-card.scss'
})
export class StatCardComponent {
  @Input() label = '';
  @Input() value = '';
  @Input() icon = '';
  @Input() trend = '';
  @Input() trendIcon = '';
  @Input() trendTone: 'positive' | 'negative' | 'muted' = 'positive';
  @Input() tone: StatTone = 'primary';
}
