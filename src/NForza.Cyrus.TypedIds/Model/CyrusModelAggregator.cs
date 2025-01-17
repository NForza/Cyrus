using NForza.Cyrus.TypedIds;

namespace NForza.Cyrus.TypedIds.Model
{
    internal class CyrusModelAggregator(IEnumerable<ICyrusModel> models) : ICyrusModel
    {
        public IEnumerable<string> Strings => models.SelectMany(m => m.Strings).Distinct();
        public IEnumerable<string> Integers => models.SelectMany(m => m.Integers).Distinct();
        public IEnumerable<string> Guids => models.SelectMany(m => m.Guids).Distinct();
        public IEnumerable<ModelDefinition> Events => models.SelectMany(m => m.Events).Distinct(ModelDefinitionEqualityComparer.Instance);
        public IEnumerable<ModelDefinition> Commands => models.SelectMany(m => m.Commands).Distinct(ModelDefinitionEqualityComparer.Instance);
    }
}