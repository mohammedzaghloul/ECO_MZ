import { Component, Inject, OnInit, PLATFORM_ID, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../../core/Services/account.service';
import { ToastrService } from 'ngx-toastr';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment.development';
import { LanguageService } from '../../core/Services/language.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  formGroup: FormGroup;
  isLoading = signal(false);
  loginError = signal('');
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private toastr: ToastrService,
    private languageService: LanguageService,
    private router: Router,
    private route: ActivatedRoute,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    this.formGroup = this.fb.group({
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required]],
    });
  }

  get _Email() { return this.formGroup.get('Email'); }
  get _Password() { return this.formGroup.get('Password'); }

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      const socialError = this.route.snapshot.queryParamMap.get('socialError');
      if (socialError === 'notconfigured') {
        this.toastr.error(
          this.languageService.t('AUTH_TOAST_GOOGLE_NOT_CONFIGURED'),
          this.languageService.t('AUTH_TOAST_GOOGLE_TITLE')
        );
      } else if (socialError) {
        this.toastr.error(
          this.languageService.t('AUTH_TOAST_GOOGLE_ERROR'),
          this.languageService.t('AUTH_TOAST_GOOGLE_TITLE')
        );
      }
    }
  }

  loginWithGoogle(): void {
    if (isPlatformBrowser(this.platformId)) {
      window.location.href = `${environment.authApiUrl}Account/google-challenge`;
    }
  }

  Submit() {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      return;
    }
    this.loginError.set('');
    this.isLoading.set(true);
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');

    this.accountService.login({
      email: this.formGroup.value.Email,
      password: this.formGroup.value.Password,
    }).subscribe({
      next: (res: any) => {
        this.isLoading.set(false);
        const message = typeof res?.message === 'string' ? res.message : '';
        if (message.toLowerCase().includes('confirm')) {
          if (isPlatformBrowser(this.platformId)) {
            this.toastr.warning(
              this.languageService.t('AUTH_TOAST_EMAIL_NOT_CONFIRMED'),
              this.languageService.t('AUTH_TOAST_EMAIL_NOT_CONFIRMED_TITLE')
            );
          }
          return;
        }
        if (isPlatformBrowser(this.platformId)) {
          this.toastr.success(this.languageService.t('AUTH_TOAST_LOGIN_SUCCESS'));
        }
        this.accountService.getCurrentUser().subscribe({
          next: (userResponse) => {
            const user = userResponse.data;
            this.accountService.currentUser.set(user);
            const isAdmin = user?.roles?.some((role) => role.trim().toLowerCase() === 'admin') ?? false;
            const destination = returnUrl?.startsWith('/')
              ? returnUrl
              : isAdmin
                ? '/admin'
                : '/';
            this.router.navigateByUrl(destination);
          },
          error: () => {
            this.toastr.error(
              this.languageService.t('AUTH_TOAST_SESSION_ERROR'),
              this.languageService.t('AUTH_TOAST_LOGIN_FAILED')
            );
          }
        });
      },
      error: (err) => {
        this.isLoading.set(false);
        const message = err?.error?.message ?? this.languageService.t('AUTH_TOAST_INVALID_CREDENTIALS');
        this.loginError.set(message);
        if (isPlatformBrowser(this.platformId)) {
          this.toastr.error(message, this.languageService.t('AUTH_TOAST_LOGIN_FAILED'));
        }
      },
    });
  }
}
