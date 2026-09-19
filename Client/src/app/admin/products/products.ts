import { Component, HostListener, OnInit, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ProductService } from '../../core/Services/product.service';
import { CategoryService } from '../../core/Services/category.service';
import { CategoryDto } from '../../shared/Models/api/category.models';
import { ProductDto } from '../../shared/Models/api/product.models';
import { LanguageService } from '../../core/Services/language.service';

const PLACEHOLDER_IMG =
  'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDQiIGhlaWdodD0iNDQiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PHJlY3Qgd2lkdGg9IjQ0IiBoZWlnaHQ9IjQ0IiBmaWxsPSIjRjFGNUY5Ii8+PHRleHQgeD0iNTAlIiB5PSI1NSUiIGZvbnQtc2l6ZT0iMTAiIGZpbGw9Ijk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSI+PC90ZXh0Pjwvc3ZnPg==';

@Component({
  selector: 'app-admin-products',
  standalone: false,
  templateUrl: './products.html',
  styleUrls: ['./products.scss']
})
export class AdminProducts implements OnInit {
  products = signal<ProductDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  totalCount = signal(0);
  pageNumber = signal(1);
  pageSize = 10;
  search = signal('');
  categoryFilter = signal<number | null>(null);
  loading = signal(true);
  saving = signal(false);
  showForm = signal(false);
  submitAttempted = signal(false);
  categoryMenuOpen = signal(false);
  editingId: number | null = null;
  selectedFiles: File[] = [];
  previews = signal<string[]>([]);
  existingPhotos = signal<string[]>([]);
  isDragOver = signal(false);
  readonly categoryMap = signal<Map<number, string>>(new Map());
  productForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private toastr: ToastrService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      newPrice: [null, [Validators.required, Validators.min(0)]],
      oldPrice: [null, Validators.min(0)],
      categoryId: [null, Validators.required],
      trackStock: [false],
      stockQuantity: [null, [Validators.min(0)]],
      sizeInventory: [''],
      lengthCm: [null, Validators.min(0)],
      widthCm: [null, Validators.min(0)],
      heightCm: [null, Validators.min(0)],
      weightKg: [null, Validators.min(0)],
      specifications: ['']
    });
    this.categoryService.getAll().subscribe((response) => {
      const categories = response.data ?? [];
      this.categories.set(categories);
      this.categoryMap.set(new Map(categories.map((category) => [category.id, category.name])));
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.productService
      .getAll({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize,
        search: this.search().trim() || undefined,
        categoryId: this.categoryFilter() ?? undefined,
      })
      .subscribe({
        next: (result) => {
          // The API wraps the page in `data` (PaginatedResult.products is a legacy name).
          const payload = result as any;
          this.products.set(payload?.data ?? payload?.products ?? []);
          this.totalCount.set(payload?.totalCount ?? 0);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.toastr.error(
            this.languageService.t('ADMIN_PRODUCTS_LOAD_ERROR'),
            this.languageService.t('ADMIN_PRODUCTS')
          );
        }
      });
  }

  get pageCount(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.pageCount) return;
    this.pageNumber.set(page);
    this.load();
  }

  searchTimeout: any;

  onSearchInput(value: string): void {
    this.search.set(value);
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.pageNumber.set(1);
      this.load();
    }, 400);
  }

  onCategoryFilter(categoryId: string): void {
    this.categoryFilter.set(categoryId ? Number(categoryId) : null);
    this.pageNumber.set(1);
    this.load();
  }

  categoryName(name: string): string {
    return this.languageService.value(name);
  }

  selectedCategoryName(): string {
    const categoryId = this.productForm?.get('categoryId')?.value;
    const category = this.categories().find((item) => item.id === categoryId);
    return category ? this.categoryName(category.name) : this.languageService.t('ADMIN_PRODUCT_SELECT_CATEGORY');
  }

  isClothingCategory(): boolean {
    const category = this.selectedCategoryName().toLowerCase();
    return /ملابس|أزياء|fashion|clothing|apparel/.test(category);
  }

  toggleCategoryMenu(): void {
    this.categoryMenuOpen.update((isOpen) => !isOpen);
  }

  selectCategory(categoryId: number): void {
    this.productForm.get('categoryId')?.setValue(categoryId);
    this.productForm.get('categoryId')?.markAsTouched();
    this.categoryMenuOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  closeCategoryMenu(): void {
    this.categoryMenuOpen.set(false);
  }

  showError(controlName: string): boolean {
    const control = this.productForm.get(controlName);
    return !!control && control.invalid && (control.touched || this.submitAttempted());
  }

  chooseFiles(event: Event): void {
    this.setSelectedFiles(Array.from((event.target as HTMLInputElement).files ?? []));
  }

  onFilesDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onFilesDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
  }

  onFilesDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    this.setSelectedFiles(Array.from(event.dataTransfer?.files ?? []));
  }

  private setSelectedFiles(files: File[]): void {
    this.selectedFiles = files.filter((file) => file.type.startsWith('image/'));
    this.clearPreviews();
    this.previews.set(this.selectedFiles.map((file) => URL.createObjectURL(file)));
  }

  private clearPreviews(): void {
    this.previews().forEach((url) => URL.revokeObjectURL(url));
    this.previews.set([]);
  }

  save(): void {
    this.submitAttempted.set(true);
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      this.toastr.warning(this.languageService.t('ADMIN_PRODUCT_FORM_ERROR'), this.languageService.t('ADMIN_PRODUCTS'));
      return;
    }
    if (!this.editingId && !this.selectedFiles.length) {
      this.toastr.warning(this.languageService.t('ADMIN_PRODUCT_IMAGE_REQUIRED'), this.languageService.t('ADMIN_PRODUCTS'));
      return;
    }
    if (this.productForm.value.trackStock && this.productForm.value.stockQuantity == null) {
      this.toastr.warning(this.languageService.t('ADMIN_PRODUCT_STOCK_REQUIRED'), this.languageService.t('ADMIN_PRODUCTS'));
      return;
    }
    try {
      const specifications = JSON.parse(this.productForm.value.specifications || '[]');
      if (!Array.isArray(specifications) || specifications.some((item) => !item?.label || !item?.value)) {
        throw new Error();
      }
      const sizeInventory = this.parseSizeInventory(this.productForm.value.sizeInventory);
      if (this.isClothingCategory() && Object.keys(sizeInventory).length > 0) {
        const withoutSizeSpecs = specifications.filter((item) =>
          !/^(المقاسات|مخزون المقاسات|sizes|size stock)$/i.test(String(item.label).trim()));
        withoutSizeSpecs.push(
          { label: 'المقاسات', value: Object.keys(sizeInventory).join(', '), sortOrder: withoutSizeSpecs.length },
          { label: 'مخزون المقاسات', value: JSON.stringify(sizeInventory), sortOrder: withoutSizeSpecs.length + 1 }
        );
        this.productForm.patchValue({ specifications: JSON.stringify(withoutSizeSpecs) });
      }
    } catch {
      this.toastr.warning(this.languageService.t('ADMIN_PRODUCT_SPECS_ERROR'), this.languageService.t('ADMIN_PRODUCTS'));
      return;
    }
    const data = new FormData();
    Object.entries(this.productForm.value).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        data.append(key, String(value));
      }
    });
    this.selectedFiles.forEach((file) => data.append('Photos', file));
    this.saving.set(true);
    const request: Observable<unknown> = this.editingId
      ? this.productService.update(this.editingId, data)
      : this.productService.create(data);
    request.subscribe({
      next: () => {
        this.toastr.success(
          this.languageService.t(this.editingId ? 'ADMIN_PRODUCT_UPDATED' : 'ADMIN_PRODUCT_CREATED'),
          this.languageService.t('ADMIN_PRODUCTS')
        );
        this.reset();
      },
      error: (error) => {
        this.saving.set(false);
        this.toastr.error(error?.error?.message ?? this.languageService.t('ADMIN_PRODUCT_SAVE_ERROR'), this.languageService.t('ADMIN_PRODUCTS'));
      }
    });
  }

  edit(product: ProductDto): void {
    this.editingId = product.id;
    this.submitAttempted.set(false);
    this.categoryMenuOpen.set(false);
    this.productForm.patchValue(product);
    this.productForm.patchValue({
      specifications: JSON.stringify(product.specifications ?? [], null, 2)
    });
    const sizeStock = product.specifications?.find(item => /مخزون المقاسات|size stock/i.test(item.label));
    const sizes = product.specifications?.find(item => /^المقاسات$|^sizes$/i.test(item.label));
    let sizeInventory = '';
    if (sizeStock) {
      try {
        const values = JSON.parse(sizeStock.value) as Record<string, number>;
        sizeInventory = Object.entries(values).map(([size, stock]) => `${size}:${stock}`).join('\n');
      } catch {
        sizeInventory = '';
      }
    } else if (sizes) {
      sizeInventory = sizes.value.split(/[,،]/).map(size => `${size.trim()}:0`).join('\n');
    }
    this.productForm.patchValue({ sizeInventory });
    this.selectedFiles = [];
    this.isDragOver.set(false);
    this.clearPreviews();
    this.existingPhotos.set((product.photos ?? []).map((photo) => this.photoUrlValue(photo)));
    this.showForm.set(true);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  remove(id: number): void {
    if (!confirm(this.languageService.t('ADMIN_PRODUCT_DELETE_CONFIRM'))) return;
    this.productService.delete(id).subscribe({
      next: () => {
        this.toastr.success(this.languageService.t('ADMIN_PRODUCT_DELETED'), this.languageService.t('ADMIN_PRODUCTS'));
        this.load();
      },
      error: () => this.toastr.error(this.languageService.t('ADMIN_PRODUCT_DELETE_ERROR'), this.languageService.t('ADMIN_PRODUCTS'))
    });
  }

  reset(): void {
    this.editingId = null;
    this.selectedFiles = [];
    this.isDragOver.set(false);
    this.clearPreviews();
    this.existingPhotos.set([]);
    this.productForm.reset({
      name: '',
      description: '',
      newPrice: null,
      oldPrice: null,
      categoryId: null,
      trackStock: false,
      stockQuantity: null,
      lengthCm: null,
      widthCm: null,
      heightCm: null,
      weightKg: null,
      specifications: '',
      sizeInventory: ''
    });
    this.saving.set(false);
    this.submitAttempted.set(false);
    this.categoryMenuOpen.set(false);
    this.productForm.markAsPristine();
    this.productForm.markAsUntouched();
    this.showForm.set(false);
    this.load();
  }

  private parseSizeInventory(value: string | null | undefined): Record<string, number> {
    return String(value ?? '').split(/\r?\n/).reduce<Record<string, number>>((result, line) => {
      const [rawSize, rawStock] = line.split(':');
      const size = rawSize?.trim();
      const stock = Number(rawStock?.trim());
      if (size && Number.isInteger(stock) && stock >= 0) result[size] = stock;
      return result;
    }, {});
  }

  toggleForm(): void {
    if (this.showForm()) {
      this.reset();
    } else {
      this.submitAttempted.set(false);
      this.productForm.markAsPristine();
      this.productForm.markAsUntouched();
      this.showForm.set(true);
    }
  }

  /** First product photo as a servable URL (photos store bare filenames). */
  photoUrl(product: ProductDto): string {
    const photo = product.photos?.[0];
    if (!photo) return PLACEHOLDER_IMG;
    return this.photoUrlValue(photo);
  }

  private photoUrlValue(photo: string): string {
    if (photo.startsWith('http')) return photo;
    const normalised = photo.replace(/\\/g, '/');
    return normalised.includes('Images/')
      ? `/${normalised}`
      : `/Images/Products/${normalised}`;
  }
}
