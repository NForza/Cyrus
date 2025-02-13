using System.Collections.Generic;
using System.Linq;

namespace NForza.Cyrus.Abstractions.Model
{
    public class CyrusModelBase: ICyrusModel
    {
        public virtual IEnumerable<string> Guids { get; set; } = Enumerable.Empty<string>();
        public virtual IEnumerable<string> Integers { get; set; } = Enumerable.Empty<string>();
        public virtual IEnumerable<string> Strings { get; set; } = Enumerable.Empty<string>();
        public virtual IEnumerable<ModelTypeDefinition> Models { get; set; } = Enumerable.Empty<ModelTypeDefinition>();
        public virtual IEnumerable<ModelTypeDefinition> Events { get; set; } = Enumerable.Empty<ModelTypeDefinition>();
        public virtual IEnumerable<ModelTypeDefinition> Commands { get; set; } = Enumerable.Empty<ModelTypeDefinition>();
        public virtual IEnumerable<ModelTypeDefinition> Queries { get; set; } = Enumerable.Empty<ModelTypeDefinition>();
        public virtual IEnumerable<ModelHubDefinition> Hubs { get; set; } = Enumerable.Empty<ModelHubDefinition>();
    }
}
