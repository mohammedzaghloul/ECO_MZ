import { Component } from '@angular/core';
import { BasketService } from '../../../basket/basket.service';
import { AccountService } from '../../../core/Services/account.service';
import { LanguageService } from '../../../core/Services/language.service';

@Component({
  selector: 'app-bottom-nav',
  standalone: false,
  templateUrl: './bottom-nav.component.html',
  styleUrls: ['./bottom-nav.component.scss']
})
export class BottomNavComponent {
  constructor(
    public basketService: BasketService,
    public accountService: AccountService,
    public languageService: LanguageService
  ) {}

  /** Keep the shared auth state fresh when the bar first renders. */
  ngOnInit(): void {
    // HeaderComponent owns the single initial current-user request.
  }

  get accountLink(): string {
    return this.accountService.currentUser() ? '/profile' : '/account/login';
  }
}
