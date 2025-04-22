import { CustomerId } from './CustomerId';
import { Name } from './Name';
import { Address } from './Address';
import { CustomerType } from './CustomerType';
export interface AddCustomerCommand {
    id: CustomerId;
    name: Name;
    address: Address;
    customerType: CustomerType;
}
