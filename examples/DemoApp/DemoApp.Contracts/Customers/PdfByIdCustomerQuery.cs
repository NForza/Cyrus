using NForza.Cyrus.Abstractions;

namespace DemoApp.Domain.Customer
{
    [Query]
    public record CustomerTemplateQuery(string terms);
}