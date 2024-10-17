using ConsoleApp40.Contracts;

namespace ConsoleApp40.Domain;

public class AddCustomerCommandHandler
{
    public int Execute(AddCustomerCommand command)
    {
        Console.WriteLine($"Customer created: {command.Name}, {command.Address}");
        return 42;
    }
 }
