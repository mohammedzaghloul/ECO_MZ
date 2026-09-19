import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { environment } from '../../../environments/environment.development';
import { firstValueFrom } from 'rxjs';

type OfflineOperationKind = 'basket' | 'basket-snapshot' | 'guest-order';

interface OfflineOperation {
  key: string;
  kind: OfflineOperationKind;
  payload: unknown;
  createdAt: number;
  attempts: number;
}

const DATABASE_NAME = 'eco-offline';
const STORE_NAME = 'operations';

@Injectable({ providedIn: 'root' })
export class OfflineSupportService {
  readonly online = signal(true);
  readonly apiAvailable = signal(true);
  readonly pendingCount = signal(0);
  private readonly browser: boolean;
  private databasePromise?: Promise<IDBDatabase>;
  private flushInProgress = false;
  private healthTimer?: ReturnType<typeof setInterval>;

  constructor(
    private readonly http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object,
  ) {
    this.browser = isPlatformBrowser(platformId);
    if (!this.browser) return;

    this.online.set(navigator.onLine);
    window.addEventListener('online', () => {
      this.online.set(true);
      void this.refreshStatusAndFlush();
    });
    window.addEventListener('offline', () => {
      this.online.set(false);
      this.apiAvailable.set(false);
    });
    this.healthTimer = setInterval(() => void this.refreshStatusAndFlush(), 30000);
    void this.refreshStatusAndFlush();
  }

  async queueBasket(payload: unknown, basketId: string): Promise<void> {
    await this.put({ key: `basket:${basketId}`, kind: 'basket', payload, createdAt: Date.now(), attempts: 0 });
  }

  async queueGuestOrder(payload: unknown, key: string): Promise<void> {
    await this.put({ key: `guest-order:${key}`, kind: 'guest-order', payload, createdAt: Date.now(), attempts: 0 });
  }

  async cacheBasket(snapshot: unknown, basketId: string): Promise<void> {
    await this.put({
      key: `basket-snapshot:${basketId}`,
      kind: 'basket-snapshot',
      payload: snapshot,
      createdAt: Date.now(),
      attempts: 0,
    });
  }

  async getCachedBasket<T>(basketId: string): Promise<T | null> {
    if (!this.browser) return null;
    const database = await this.openDatabase();
    return new Promise((resolve, reject) => {
      const request = database.transaction(STORE_NAME, 'readonly').objectStore(STORE_NAME).get(`basket-snapshot:${basketId}`);
      request.onsuccess = () => resolve((request.result as OfflineOperation | undefined)?.payload as T ?? null);
      request.onerror = () => reject(request.error ?? new Error('Unable to read cached basket.'));
    });
  }

  async refreshStatusAndFlush(): Promise<void> {
    if (!this.browser || !navigator.onLine) {
      this.online.set(false);
      this.apiAvailable.set(false);
      return;
    }

    this.online.set(true);
    try {
      await firstValueFrom(this.http.get(`${environment.basurl}Product/GetAll`, {
        params: {
          PageNumber: 1,
          PageSize: 1,
          offlineHealthCheck: Date.now(),
          'ngsw-bypass': 'true',
        },
        headers: { 'Cache-Control': 'no-cache' },
      }));
      this.apiAvailable.set(true);
      await this.flush();
    } catch {
      this.apiAvailable.set(false);
      await this.updatePendingCount();
    }
  }

  describeFailure(error: unknown): string {
    if (!this.online()) return 'لا يوجد اتصال بالإنترنت. تم حفظ البيانات وسيتم إرسالها تلقائيًا عند عودة الاتصال.';
    if (!this.apiAvailable() || (error instanceof HttpErrorResponse && (error.status === 0 || error.status >= 500))) {
      return 'الإنترنت يعمل لكن الخادم غير متاح حاليًا. تم حفظ البيانات وسيتم إعادة المحاولة تلقائيًا.';
    }
    return 'تعذر تنفيذ العملية. راجع البيانات وحاول مرة أخرى.';
  }

  private async flush(): Promise<void> {
    if (this.flushInProgress || !this.browser || !navigator.onLine) return;
    this.flushInProgress = true;
    try {
      const operations = await this.readAll();
      for (const operation of operations) {
        if (operation.kind === 'basket-snapshot') continue;
        try {
          if (operation.kind === 'basket') {
            await firstValueFrom(this.http.post(`${environment.basurl}Basket`, operation.payload, { withCredentials: true }));
          } else {
            await firstValueFrom(this.http.post(`${environment.basurl}GuestOrder/Create`, operation.payload));
          }
          await this.remove(operation.key);
        } catch (error) {
          await this.updateAttempt(operation);
          if (error instanceof HttpErrorResponse && error.status >= 400 && error.status < 500) {
            await this.remove(operation.key);
          }
          break;
        }
      }
    } finally {
      this.flushInProgress = false;
      await this.updatePendingCount();
    }
  }

  private async put(operation: OfflineOperation): Promise<void> {
    if (!this.browser) return;
    const database = await this.openDatabase();
    await new Promise<void>((resolve, reject) => {
      const transaction = database.transaction(STORE_NAME, 'readwrite');
      transaction.objectStore(STORE_NAME).put(operation);
      transaction.oncomplete = () => resolve();
      transaction.onerror = () => reject(transaction.error ?? new Error('Unable to save offline operation.'));
    });
    await this.updatePendingCount();
  }

  private async readAll(): Promise<OfflineOperation[]> {
    const database = await this.openDatabase();
    return new Promise((resolve, reject) => {
      const request = database.transaction(STORE_NAME, 'readonly').objectStore(STORE_NAME).getAll();
      request.onsuccess = () => resolve(request.result as OfflineOperation[]);
      request.onerror = () => reject(request.error ?? new Error('Unable to read offline operations.'));
    });
  }

  private async remove(key: string): Promise<void> {
    const database = await this.openDatabase();
    await new Promise<void>((resolve, reject) => {
      const transaction = database.transaction(STORE_NAME, 'readwrite');
      transaction.objectStore(STORE_NAME).delete(key);
      transaction.oncomplete = () => resolve();
      transaction.onerror = () => reject(transaction.error ?? new Error('Unable to remove offline operation.'));
    });
  }

  private async updateAttempt(operation: OfflineOperation): Promise<void> {
    await this.put({ ...operation, attempts: operation.attempts + 1 });
  }

  private async updatePendingCount(): Promise<void> {
    if (!this.browser) return;
    try {
      this.pendingCount.set((await this.readAll()).length);
    } catch {
      this.pendingCount.set(0);
    }
  }

  private openDatabase(): Promise<IDBDatabase> {
    if (this.databasePromise) return this.databasePromise;
    this.databasePromise = new Promise((resolve, reject) => {
      const request = indexedDB.open(DATABASE_NAME, 1);
      request.onupgradeneeded = () => request.result.createObjectStore(STORE_NAME, { keyPath: 'key' });
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error ?? new Error('Unable to open offline storage.'));
    });
    return this.databasePromise;
  }
}
