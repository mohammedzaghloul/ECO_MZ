import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../core/Services/account.service';

@Component({
  selector: 'app-forgot-password',
  standalone: false,
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword {
  submitted = false;
  loading = false;
  form: FormGroup;

  constructor(private fb: FormBuilder, private accountService: AccountService) {
    this.form = this.fb.group({ email: ['', [Validators.required, Validators.email]] });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.accountService.sendEmailOrGetPassword(this.form.value.email!).subscribe({
      next: () => this.finish(),
      error: () => this.finish(),
    });
  }

  private finish(): void {
    this.loading = false;
    this.submitted = true;
  }
}
