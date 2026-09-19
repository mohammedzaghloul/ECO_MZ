import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { IdentityService } from '../Identity.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-register-stepper',
  standalone: false,
  templateUrl: './register-stepper.component.html',
  styleUrl: './register-stepper.component.scss'
})
export class RegisterStepperComponent {
  currentStep = signal(0);
  steps = ['Personal Info', 'Address', 'Confirmation'];
  isLoading = signal(false);

  personalInfoForm: FormGroup;
  addressForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private identityService: IdentityService,
    private router: Router,
    private http: HttpClient
  ) {
    this.personalInfoForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.addressForm = this.fb.group({
      street: ['', [Validators.required]],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
      zipCode: ['', [Validators.required]],
      country: ['', [Validators.required]]
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onStepChange(step: number): void {
    this.currentStep.set(step);
  }

  nextStep(): void {
    if (this.currentStep() === 0 && !this.personalInfoForm.valid) {
      this.personalInfoForm.markAllAsTouched();
      return;
    }
    if (this.currentStep() === 1 && !this.addressForm.valid) {
      this.addressForm.markAllAsTouched();
      return;
    }
    if (this.currentStep() < this.steps.length - 1) {
      this.currentStep.set(this.currentStep() + 1);
    }
  }

  previousStep(): void {
    if (this.currentStep() > 0) {
      this.currentStep.set(this.currentStep() - 1);
    }
  }

  submitRegistration(): void {
    if (!this.personalInfoForm.valid || !this.addressForm.valid) {
      return;
    }

    this.isLoading.set(true);
    const registrationData = {
      userName: `${this.personalInfoForm.value.firstName} ${this.personalInfoForm.value.lastName}`,
      email: this.personalInfoForm.value.email,
      password: this.personalInfoForm.value.password
    };

    this.identityService.register(registrationData).subscribe({
      next: async () => {
        // Auto-login after successful registration
        const loginData = {
          email: this.personalInfoForm.value.email,
          password: this.personalInfoForm.value.password
        };

        this.identityService.login(loginData).subscribe({
          next: async () => {
            // After successful login, update the address using email
            const addressData = {
              firstName: this.personalInfoForm.value.firstName,
              lastName: this.personalInfoForm.value.lastName,
              street: this.addressForm.value.street,
              city: this.addressForm.value.city,
              state: this.addressForm.value.state,
              zipCode: this.addressForm.value.zipCode,
              country: this.addressForm.value.country
            };

            try {
              await this.http.post(`https://localhost:7144/api/Account/UpdateAddressByEmail?email=${encodeURIComponent(this.personalInfoForm.value.email)}`, addressData).toPromise();
            } catch (error) {
              console.error('Address update error:', error);
            }

            this.isLoading.set(false);
            this.router.navigate(['/account/active']);
          },
          error: (loginError) => {
            console.error('Auto-login error:', loginError);
            // Even if login fails, proceed to activation page
            this.isLoading.set(false);
            this.router.navigate(['/account/active']);
          }
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('Registration error:', error);
      }
    });
  }

  get personalInfoErrors() {
    return this.personalInfoForm.errors;
  }

  get passwordMismatch() {
    return this.personalInfoForm.errors?.['passwordMismatch'];
  }
}
