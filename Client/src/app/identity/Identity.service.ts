import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { IActiveAccount } from '../shared/Models/ActiveAccount/ActiveAccount';

@Injectable({
  providedIn: 'root'
})
export class IdentityService {
basurl = environment.basurl
  constructor(private http: HttpClient) {
  }
  register(form: any) {
    return this.http.post(this.basurl + 'Account/register', form, { withCredentials: true });
  }
  login(form: any) {
    return this.http.post(this.basurl + 'Account/Login', form, { withCredentials: true });
  }
  active(param:IActiveAccount){
     return this.http.post(this.basurl + 'Account/ActiveAccount', param);
  }

  requestPasswordReset(email: string) {
    return this.http.get(this.basurl + 'Account/Send-Email-Or-Get-Password', { params: { email } });
  }
}
