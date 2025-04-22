import { CustomerId } from './CustomerId';
import { Name } from './Name';
import { Address } from './Address';
export interface UpdateCustomerCommand {
    id: CustomerId;
    name: Name;
    address: Address;
}
