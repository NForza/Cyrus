import { Component } from '@angular/core';
import { CustomerHubService } from './generated/CustomerHub';
import { CustomerType } from './generated/CustomerType';
import { CustomerAddedEvent } from './generated/CustomerAddedEvent';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'CyrusApp';
  
  public messages: CustomerAddedEvent[] = [];

  constructor(private signalRService: CustomerHubService) {
    signalRService.startConnection();
    signalRService.onCustomerAdded.subscribe(e => this.addToList(e));
  }

  private addToList(c: CustomerAddedEvent): void {
    this.messages = [
      ...this.messages,                                            
      c
    ];
  }

  ngOnInit(): void {
    this.signalRService.startConnection();
  }

  sendMessage(): void {
    this.signalRService.addCustomerCommand(
      { 
        customerType: CustomerType.Company,
        id: crypto.randomUUID(),
        name: "name1", 
        address: { street: "Street", streetNumber: 1 }});
  }
}
