import { Component } from '@angular/core';
import { CustomerHubService } from './generated/CustomerHub';
import { CustomerType } from './generated/CustomerType';
import { CustomerAddedEvent } from './generated/CustomerAddedEvent';
import { Customer } from './generated/Customer';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'CyrusApp';
  
  public messages: CustomerAddedEvent[] = [];
  public customers: Customer[] = [];
  private count: number = 1;

  constructor(private customerHub: CustomerHubService) {
    customerHub.startConnection();
    customerHub.onCustomerAdded.subscribe(e => this.addToList(e));
    customerHub.allCustomersQueryResult.subscribe(r => this.customers = r);
  }

  private addToList(c: CustomerAddedEvent): void {
    this.messages = [
      ...this.messages,                                            
      c
    ];
  }

  ngOnInit(): void {
    this.customerHub.startConnection();
  }

  addCustomer(): void {
    this.customerHub.addCustomerCommand(
      { 
        customerType: CustomerType.Company,
        id: crypto.randomUUID(),
        name: "name1", 
        address: { street: "Street", streetNumber: 1 }
      });
  }

  allCustomers(): void {
    this.customerHub.allCustomersQuery({page: this.count, pageSize: 10});
    this.count += 10;
  }  
}
