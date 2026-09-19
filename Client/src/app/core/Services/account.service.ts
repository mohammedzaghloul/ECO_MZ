import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, finalize, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ActiveAccountDto, LoginDto, RegisterDto, UserDto } from '../../shared/Models/api/account.models';
import { AddressDto } from '../../shared/Models/api/account.models';
import { ApiEnvelope } from '../../shared/Models/api/common.models';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly baseUrl = `${environment.basurl}Account`;
  /** Shared auth state so the header reflects login without a page reload. */
  readonly currentUser = signal<UserDto | null>(null);
  private currentUserRequest$: Observable<ApiEnvelope<UserDto>> | null = null;

  constructor(private http: HttpClient) {}

  refreshUser(): void {
    if (this.currentUserRequest$) return;

    this.currentUserRequest$ = this.getCurrentUser().pipe(
      shareReplay({ bufferSize: 1, refCount: false }),
      finalize(() => this.currentUserRequest$ = null)
    );
    this.currentUserRequest$.subscribe({
      next: (response) => this.currentUser.set(response?.data ?? null),
      error: () => this.currentUser.set(null)
    });
  }

  register(dto: RegisterDto): Observable<ApiEnvelope<never>> {
    return this.http.post<ApiEnvelope<never>>(`${this.baseUrl}/register`, dto, { withCredentials: true });
  }

  login(dto: LoginDto): Observable<ApiEnvelope<never>> {
    return this.http.post<ApiEnvelope<never>>(`${this.baseUrl}/Login`, dto, { withCredentials: true });
  }

  activateAccount(dto: ActiveAccountDto): Observable<ApiEnvelope<never>> {
    return this.http.post<ApiEnvelope<never>>(`${this.baseUrl}/ActiveAccount`, dto);
  }

  sendEmailOrGetPassword(email: string): Observable<ApiEnvelope<never>> {
    return this.http.get<ApiEnvelope<never>>(`${this.baseUrl}/Send-Email-Or-Get-Password`, {
      params: new HttpParams().set('email', email),
    });
  }

  resetPassword(dto: { email: string; token: string; password: string }): Observable<ApiEnvelope<never>> {
    return this.http.post<ApiEnvelope<never>>(`${this.baseUrl}/Reset-Password`, dto);
  }

  getAddress(): Observable<ApiEnvelope<AddressDto | null>> {
    return this.http.get<ApiEnvelope<AddressDto | null>>(`${this.baseUrl}/GetAddress`, { withCredentials: true });
  }

  updateAddress(dto: AddressDto): Observable<ApiEnvelope<AddressDto>> {
    return this.http.post<ApiEnvelope<AddressDto>>(`${this.baseUrl}/UpdateAddress`, dto, { withCredentials: true });
  }

  logout(): Observable<ApiEnvelope<never>> {
    return this.http.post<ApiEnvelope<never>>(`${this.baseUrl}/Logout`, {}, { withCredentials: true });
  }

  getCurrentUser(): Observable<ApiEnvelope<UserDto>> {
    return this.http.get<ApiEnvelope<UserDto>>(`${this.baseUrl}/GetCurrentUser`, { withCredentials: true });
  }
}
