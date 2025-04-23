import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { AllCustomersQuery } from './AllCustomersQuery';
import { AddCustomerCommand } from './AddCustomerCommand';
import { DeleteCustomerCommand } from './DeleteCustomerCommand';
import { Customer } from './Customer';
import { CustomerUpdatedEvent } from './CustomerUpdatedEvent';
import { CustomerAddedEvent } from './CustomerAddedEvent';

@Injectable({
  providedIn: 'root',
})
export class CustomerHubService {
  private hubConnection: signalR.HubConnection;
  private _allCustomersQueryResult: BehaviorSubject<Customer[]> = new BehaviorSubject<Customer[]>([]);
  private _customerUpdatedEventSubject: Subject<CustomerUpdatedEvent> = new Subject<CustomerUpdatedEvent>();
  private _customerAddedEventSubject: Subject<CustomerAddedEvent> = new Subject<CustomerAddedEvent>();

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7166/customer-hub') 
      .configureLogging(signalR.LogLevel.Information)
      .build();
    this.hubConnection.on('customerUpdatedEvent', (data: CustomerUpdatedEvent) => {
      console.log('Received CustomerUpdatedEvent:', data);
      this._customerUpdatedEventSubject.next(data);
    });
    this.hubConnection.on('customerAddedEvent', (data: CustomerAddedEvent) => {
      console.log('Received CustomerAddedEvent:', data);
      this._customerAddedEventSubject.next(data);
    });  
    this.hubConnection.on('allCustomersQueryResult', (data : Customer[]) => {
      console.log('Received :', data);
      this._allCustomersQueryResult.next(data);
    });
    this.hubConnection.onclose((error) => {
      console.error('SignalR connection closed:', error);
    });
  }

  public startConnection(): void {
    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR Connected');
      })
      .catch((err) => {
        console.error('SignalR Connection Error:', err);
      });
  }  
  public addCustomerCommand(command: AddCustomerCommand ): void {
    console.log('Sending addCustomerCommand');           
    this.hubConnection.invoke('addCustomerCommand', command).catch((err) => {
      console.error('Error sending addCustomerCommand:', err);
    });
  }  
  public deleteCustomerCommand(command: DeleteCustomerCommand ): void {
    console.log('Sending deleteCustomerCommand');           
    this.hubConnection.invoke('deleteCustomerCommand', command).catch((err) => {
      console.error('Error sending deleteCustomerCommand:', err);
    });
  }  
  public allCustomersQuery(query: AllCustomersQuery ): void {
    console.log('Sending allCustomersQuery');
    this.hubConnection.invoke('allCustomersQuery', query).catch((err) => {
      console.error('Error sending allCustomersQuery:', err);
    });
  }

  public get allCustomersQueryResult(): Observable<Customer[]> {
    return this._allCustomersQueryResult;
  }
  public get onCustomerUpdated(): Observable<CustomerUpdatedEvent> {
    return this._customerUpdatedEventSubject;
  }
  public get onCustomerAdded(): Observable<CustomerAddedEvent> {
    return this._customerAddedEventSubject;
  }
}