import { Component, OnInit, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { CategoryService } from '../../core/Services/category.service';
import { CategoryDto } from '../../shared/Models/api/category.models';
import { LanguageService } from '../../core/Services/language.service';

@Component({
  selector: 'app-admin-categories',
  standalone: false,
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class AdminCategories implements OnInit {
  categories = signal<CategoryDto[]>([]);
  loading = signal(true);
  editingId: number | null = null;
  saving = signal(false);
  search = signal('');
  categoryForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private toastr: ToastrService,
    private langService: LanguageService
  ) {}

  ngOnInit(): void {
    this.categoryForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      parentCategoryId: [null]
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (response) => {
        this.categories.set(response.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toastr.error(this.langService.t('ADMIN_CATEGORIES_LOAD_ERROR'), this.langService.t('ADMIN_CATEGORIES'));
      }
    });
  }

  showError(controlName: string): boolean {
    const control = this.categoryForm.get(controlName);
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  save(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      this.toastr.warning(this.langService.t('ADMIN_CATEGORIES_FILL_FIELDS'), this.langService.t('ADMIN_CATEGORIES'));
      return;
    }
    this.saving.set(true);
    const value = this.categoryForm.value;
    const request: Observable<unknown> = this.editingId
      ? this.categoryService.update(this.editingId, { id: this.editingId, ...value })
      : this.categoryService.create(value);
    request.subscribe({
      next: () => {
        this.toastr.success(this.editingId ? this.langService.t('ADMIN_CATEGORY_UPDATED') : this.langService.t('ADMIN_CATEGORY_CREATED'), this.langService.t('ADMIN_CATEGORIES'));
        this.reset();
      },
      error: (error) => {
        this.saving.set(false);
        this.toastr.error(error?.error?.message ?? this.langService.t('ADMIN_CATEGORY_SAVE_ERROR'), this.langService.t('ADMIN_CATEGORIES'));
      }
    });
  }

  edit(category: CategoryDto): void {
    this.editingId = category.id;
    this.categoryForm.patchValue(category);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  remove(id: number): void {
    if (!confirm(this.langService.t('ADMIN_CATEGORY_DELETE_CONFIRM'))) return;
    this.categoryService.delete(id).subscribe({
      next: () => {
        this.toastr.success(this.langService.t('ADMIN_CATEGORY_DELETED'), this.langService.t('ADMIN_CATEGORIES'));
        this.load();
      },
      error: () => this.toastr.error(this.langService.t('ADMIN_CATEGORY_DELETE_ERROR'), this.langService.t('ADMIN_CATEGORIES'))
    });
  }

  reset(): void {
    this.editingId = null;
    this.categoryForm.reset();
    this.saving.set(false);
    this.load();
  }

  onSearch(value: string): void {
    this.search.set(value);
  }

  filteredCategories(): CategoryDto[] {
    const term = this.search().trim().toLowerCase();
    return term
      ? this.categories().filter((category) => `${category.name} ${category.description}`.toLowerCase().includes(term))
      : this.categories();
  }
}
