import { Component, Input } from '@angular/core';

export interface TrackingStep {
  /** Translation key shown as the step title. */
  labelKey: string;
  /** Display label for the step. */
  label: string;
  date?: string;
  completed: boolean;
  current?: boolean;
}

@Component({
  selector: 'app-order-tracking',
  templateUrl: './order-tracking.component.html',
  styleUrl: './order-tracking.component.scss',
  standalone: false
})
export class OrderTrackingComponent {
  @Input() steps: TrackingStep[] = [];

  get currentStepNumber(): number {
    const current = this.steps.findIndex((step) => step.current);
    if (current >= 0) return current + 1;

    return Math.max(this.steps.filter((step) => step.completed).length, 1);
  }
}
