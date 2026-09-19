import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../core/Services/account.service';
import { Router } from '@angular/router';
import { timeout } from 'rxjs';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register implements OnInit {
  formGroup!: FormGroup;
  isLoading = signal(false);
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.formGroup = this.fb.group({
      UserName: ['', [Validators.required, Validators.minLength(3)]],
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  get _UserName() {
    return this.formGroup.get('UserName');
  }

  get _Email() {
    return this.formGroup.get('Email');
  }

  get _Password() {
    return this.formGroup.get('Password');
  }

  Submit(): void {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }
    this.isLoading.set(true);
    this.accountService.register({
      userName: this.formGroup.value.UserName,
      email: this.formGroup.value.Email,
      password: this.formGroup.value.Password,
    }).pipe(timeout(20000)).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/account/active'], {
          queryParams: { email: this.formGroup.value.Email },
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        const message = error?.name === 'TimeoutError'
          ? 'The server took too long to respond. Please try again.'
          : error?.error?.message ?? 'Unable to create your account.';
        this.formGroup.setErrors({ registerFailed: message });
      },
    });
  }
}
