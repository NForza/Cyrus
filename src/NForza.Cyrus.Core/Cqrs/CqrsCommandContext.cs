using System.Collections.Generic;

namespace NForza.Cyrus.Cqrs
{
    public class CqrsCommandContext
    {
        public List<object> Messages { get; } = new();
    }
}