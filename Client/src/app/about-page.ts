import { Component, OnDestroy, OnInit, signal } from '@angular/core';

@Component({
  selector: 'app-about-page',
  standalone: false,
  templateUrl: './about-page.html',
  styleUrl: './info-page.scss',
})
export class AboutPage implements OnInit, OnDestroy {
  readonly storyImages = [
    'images/korean_model_spring_collection.png',
    'images/luxury_bag_1781872655930.png',
    'images/luxury_hero_watch.png',
  ];

  readonly currentStory = signal(0);
  private slideshowId?: number;

  ngOnInit(): void {
    this.slideshowId = window.setInterval(() => {
      this.currentStory.update((index) => (index + 1) % this.storyImages.length);
    }, 3500);
  }

  ngOnDestroy(): void {
    if (this.slideshowId) {
      window.clearInterval(this.slideshowId);
    }
  }

  selectStory(index: number): void {
    this.currentStory.set(index);
  }
}
