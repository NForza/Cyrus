using System.Collections.Generic;
using System.Threading.Tasks;

namespace NForza.Cyrus.Cqrs;

public interface IEventBus
{
    Task Publish(IEnumerable<object> events);
}