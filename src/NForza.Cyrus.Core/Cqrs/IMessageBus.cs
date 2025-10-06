using System.Collections.Generic;
using System.Threading.Tasks;

namespace NForza.Cyrus.Cqrs;

public interface IMessageBus
{
    Task Publish(params IEnumerable<object> messages);
}