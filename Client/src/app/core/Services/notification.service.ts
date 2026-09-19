import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment.development';
import { Observable } from 'rxjs';

export interface NotificationItem {
  id: number;
  title: string;
  message: string;
  type: string;
  relatedEntityId?: number | null;
  isRead: boolean;
  createdAt: string;
}

interface NotificationResponse<T> {
  data: T;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly baseUrl = `${environment.basurl}Notification`;
  readonly unreadCount = signal(0);

  constructor(private http: HttpClient) {}

  loadUnreadCount(): void {
    this.http.get<NotificationResponse<number>>(`${this.baseUrl}/unread-count`, { withCredentials: true })
      .subscribe({
        next: response => this.unreadCount.set(Number(response?.data ?? 0))
      });
  }

  getAll(): Observable<NotificationResponse<NotificationItem[]>> {
    return this.http.get<NotificationResponse<NotificationItem[]>>(this.baseUrl, { withCredentials: true });
  }

  updateCountFromItems(items: NotificationItem[]): void {
    this.unreadCount.set(items.filter(item => !item.isRead).length);
  }

  markAsRead(id: number): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/${id}/read`, {}, { withCredentials: true });
  }

  clearAll(): Observable<NotificationResponse<number>> {
    return this.http.delete<NotificationResponse<number>>(this.baseUrl, { withCredentials: true });
  }

  decrementUnread(): void {
    this.unreadCount.update(count => Math.max(0, count - 1));
  }
}
