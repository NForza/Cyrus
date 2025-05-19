import { Injectable } from '@angular/core';
import { CustomerHubService } from './cyrus/CustomerHub'

@Injectable({
  providedIn: 'root'
})
export class CustomerService extends CustomerHubService {

  constructor() {
    super('https://localhost:7045/')
   }
}
