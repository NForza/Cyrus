import { CustomerId } from './CustomerId';
import { Name } from './Name';
export interface AddCustomerCommand {
    customerId: CustomerId;
    name: Name;
}
