import { Component, Inject, OnInit, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { IProduct } from './shared/Models/product';
import { Ipagination } from './shared/Models/Pagnation';
import { ShopService } from './shop/shop.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App implements OnInit {
  ngOnInit(): void {
    // Initialization logic if needed
  }
}
