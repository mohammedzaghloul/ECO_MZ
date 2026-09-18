import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-account-page',
  standalone: false,
  templateUrl: './account-page.html',
  styleUrl: './account-page.scss',
})
export class AccountPage {
  readonly title: string;
  readonly message: string;

  constructor(route: ActivatedRoute) {
    const page = route.snapshot.data['page'] as string | undefined;
    this.title = page === 'register' ? 'Create your account' : page === 'orders' ? 'My orders' : 'Welcome back';
    this.message =
      page === 'register'
        ? 'Registration will be available here soon.'
        : page === 'orders'
          ? 'Your orders will appear here after you sign in.'
          : 'Login will be available here soon.';
  }
}
