import { Component } from '@angular/core';
import { CustomerHubService } from './signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'CyrusApp';
  
  messages: { id: string, name: string, address: string }[] = [];

  constructor(private signalRService: CustomerHubService) {
    signalRService.startConnection();
  }

  ngOnInit(): void {

  }

  sendMessage(): void {
    this.signalRService.addCustomer({ "name": "name1", "address": "address1"});
  }
}
