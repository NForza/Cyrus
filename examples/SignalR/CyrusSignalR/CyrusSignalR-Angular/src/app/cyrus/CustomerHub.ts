import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { AddCustomerCommand } from './AddCustomerCommand';
import { CustomerCreatedEvent } from './CustomerCreatedEvent';

export class CustomerHubService {
  private hubConnection: signalR.HubConnection;
  private _customerCreatedEventSubject: Subject<CustomerCreatedEvent> = new Subject<CustomerCreatedEvent>();

  constructor(private baseUrl: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/customerhub`) 
      .configureLogging(signalR.LogLevel.Information)
      .build();
    this.hubConnection.on('customerCreatedEvent', (data: CustomerCreatedEvent) => {
      console.log('Received CustomerCreatedEvent:', data);
      this._customerCreatedEventSubject.next(data);
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
  public get onCustomerCreatedEvent(): Observable<CustomerCreatedEvent> {
    return this._customerCreatedEventSubject;
  }
}