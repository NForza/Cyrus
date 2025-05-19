import { Component } from '@angular/core';
import { CustomerService } from './customerhub.service';
import { AddCustomerCommand } from './cyrus/AddCustomerCommand';
import { CustomerCreatedEvent } from './cyrus/CustomerCreatedEvent';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'CyrusSignalR-Angular';
  customerCreatedMessage: string = '';

    constructor(private customerService: CustomerService) {
    this.customerService.startConnection();  
  }

  ngOnInit(): void {
    this.customerService.startConnection(); // Start SignalR connection
    this.customerService.onCustomerCreatedEvent.subscribe((event: CustomerCreatedEvent) => {
      this.customerCreatedMessage = `Customer created: ${event.name}`;
      console.log('Event received in component:', event);
    });
  }

  onAddCustomer(): void {
    const command: AddCustomerCommand = {
      name: 'Test Customer',
      customerId: '51f09ff1-e6e1-4203-8b6a-bb53c7ddfc82',
    };
    this.customerService.addCustomerCommand(command);
  }
}
