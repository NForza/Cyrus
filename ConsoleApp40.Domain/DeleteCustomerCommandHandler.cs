using ConsoleApp40.Contracts;

namespace ConsoleApp40.Domain;

public class DeleteCustomerCommandHandler
{
    public bool Execute(DeleteCustomerCommand command)
    {
        Console.WriteLine($"Customer deleted: {command.Id}");
        return true;
    }
}
