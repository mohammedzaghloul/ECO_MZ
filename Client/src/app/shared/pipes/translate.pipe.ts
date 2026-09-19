import { Pipe, PipeTransform } from '@angular/core';
import { LanguageService } from '../../core/Services/language.service';

@Pipe({
  name: 'translate',
  pure: false,
  standalone: false
})
export class TranslatePipe implements PipeTransform {
  constructor(private langService: LanguageService) {}

  transform(key: string, params?: Record<string, string | number>): string {
    return this.langService.t(key, params);
  }
}