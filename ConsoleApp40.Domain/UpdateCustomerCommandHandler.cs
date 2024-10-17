using ConsoleApp40.Contracts;

namespace ConsoleApp40.Domain
{
    public class UpdateCustomerCommandHandler
    {
        public bool Execute(UpdateCustomerCommand command)
        {
            Console.WriteLine($"Customer updated: {command.CustomerId}, {command.Name}, {command.Address}");
            return true;
        }
    }
}
