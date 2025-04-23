import { CustomerId } from './CustomerId';
import { Name } from './Name';
import { Address } from './Address';
export interface CustomerAddedEvent {
    id: CustomerId;
    name: Name;
    address: Address;
}
