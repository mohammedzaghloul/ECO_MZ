import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AccountService } from '../../core/Services/account.service';

@Component({
  selector: 'app-reset-password',
  standalone: false,
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
})
export class ResetPasswordPage implements OnInit {
  loading = false;
  success = false;
  errorMessage = '';
  email = '';
  token = '';
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private accountService: AccountService
  ) {
    this.form = this.fb.group(
      {
        password: ['', [Validators.required, Validators.minLength(6)]],
        confirm: ['', [Validators.required]],
      },
      { validators: this.passwordsMatch }
    );
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.email = params['email'] ?? '';
      this.token = params['code'] ?? params['token'] ?? '';
    });
  }

  private passwordsMatch(group: FormGroup): { mismatch: boolean } | null {
    const pass = group.get('password')?.value;
    const confirm = group.get('confirm')?.value;
    return pass === confirm ? null : { mismatch: true };
  }

  submit(): void {
    if (!this.email || !this.token) {
      this.errorMessage = 'This reset link is incomplete. Please request a new reset email.';
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.errorMessage = '';
    this.accountService
      .resetPassword({ email: this.email, token: this.token, password: this.form.value.password })
      .subscribe({
        next: () => {
          this.loading = false;
          this.success = true;
        },
        error: (error) => {
          this.loading = false;
          this.errorMessage =
            error?.error?.message ?? 'This link has expired or was already used. Please request a new one.';
        },
      });
  }
}
