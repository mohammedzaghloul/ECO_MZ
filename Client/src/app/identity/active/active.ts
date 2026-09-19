import { Component, OnInit } from '@angular/core';
import { ActiveAccountDto } from '../../shared/Models/api/account.models';
import { ActivatedRoute } from '@angular/router';
import { AccountService } from '../../core/Services/account.service';

@Component({
  selector: 'app-active',
  standalone: false,
  templateUrl: './active.html',
  styleUrl: './active.scss',
})
export class Active implements OnInit {
  activeParams: ActiveAccountDto = {
    email: '',
    token: ''
  };

  status: 'loading' | 'success' | 'error' = 'loading';
  message = 'Please wait while we confirm your email address.';

  constructor(private route: ActivatedRoute, private accountService: AccountService) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.activeParams.email = params['email'] ?? '';
      this.activeParams.token = params['code'] ?? params['token'] ?? '';
      if (!this.activeParams.email || !this.activeParams.token) {
        this.status = 'error';
        this.message = 'This activation link is incomplete. Please check the link sent to your email.';
        return;
      }
      this.accountService.activateAccount(this.activeParams).subscribe({
        next: () => {
          this.status = 'success';
          this.message = 'Your email has been confirmed. You can now sign in to your Eco account.';
        },
        error: (error) => {
          this.status = 'error';
          this.message = error?.error?.message ?? 'This link has expired or was already used. Please try signing in.';
        },
      });
    });
  }
}
