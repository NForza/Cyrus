import { CustomerId } from './CustomerId';
import { Name } from './Name';
export interface CustomerCreatedEvent {
    customerId: CustomerId;
    name: Name;
}
