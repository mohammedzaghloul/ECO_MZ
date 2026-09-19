import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-stepper',
  standalone: false,
  templateUrl: './stepper.component.html',
  styleUrl: './stepper.component.scss'
})
export class StepperComponent {
  @Input() currentStep = 0;
  @Input() steps: string[] = [];
  @Output() stepChange = new EventEmitter<number>();

  goToStep(index: number): void {
    if (index >= 0 && index < this.steps.length) {
      this.stepChange.emit(index);
    }
  }

  nextStep(): void {
    if (this.currentStep < this.steps.length - 1) {
      this.stepChange.emit(this.currentStep + 1);
    }
  }

  previousStep(): void {
    if (this.currentStep > 0) {
      this.stepChange.emit(this.currentStep - 1);
    }
  }

  isStepComplete(index: number): boolean {
    return index < this.currentStep;
  }

  isStepActive(index: number): boolean {
    return index === this.currentStep;
  }
}
