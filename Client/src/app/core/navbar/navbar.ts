import { Component, ElementRef, HostListener } from '@angular/core';
import { ShopService } from '../../shop/shop.service';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  visibale: boolean = false;
active: string|string[];

  constructor(
    private elementRef: ElementRef,
    public shopService: ShopService
  ) {}

  onNavbarSearch(term: string) {
    this.shopService.emitSearchInput(term);
  }

  ToggleDropDown(): void {
    this.visibale = !this.visibale;
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.visibale = false;
    }
  }
}
