import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root',
})
export class CustomerHubService {
  private hubConnection: signalR.HubConnection;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7166/customerHub') // Update the URL as per your server
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection.on('customeraddedevent', (data) => {
      console.log('Received customeraddedevent:', data);
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

  public addCustomer(command: { name: string; address: string }): void {
    console.log('Sending addCustomerCommand');
    this.hubConnection.invoke('addCustomerCommand', command).catch((err) => {
      console.error('Error sending AddCustomerCommand:', err);
    });
  }
}
