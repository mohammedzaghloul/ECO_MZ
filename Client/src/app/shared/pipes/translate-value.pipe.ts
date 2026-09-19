import { Pipe, PipeTransform } from '@angular/core';
import { LanguageService } from '../../core/Services/language.service';

@Pipe({
  name: 'translateValue',
  pure: false,
  standalone: false
})
export class TranslateValuePipe implements PipeTransform {
  constructor(private readonly language: LanguageService) {}

  transform(value: string | null | undefined): string {
    return this.language.value(value);
  }
}
