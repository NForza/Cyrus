using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

internal class CyrusModelAggregator(IEnumerable<ICyrusModel> models) : ICyrusModel
{
    public IEnumerable<string> Strings => models.SelectMany(m => m.Strings).Distinct();
    public IEnumerable<string> Integers => models.SelectMany(m => m.Integers).Distinct();
    public IEnumerable<string> Guids => models.SelectMany(m => m.Guids).Distinct();
    public IEnumerable<ModelTypeDefinition> Models => models.SelectMany(m => m.Models).Distinct(NamedTypeEqualityComparer.Instance);
    public IEnumerable<ModelTypeDefinition> Events => models.SelectMany(m => m.Events).Distinct(NamedTypeEqualityComparer.Instance);
    public IEnumerable<ModelTypeDefinition> Commands => models.SelectMany(m => m.Commands).Distinct(NamedTypeEqualityComparer.Instance);
    public IEnumerable<ModelTypeDefinition> Queries => models.SelectMany(m => m.Queries).Distinct(NamedTypeEqualityComparer.Instance);
    public IEnumerable<ModelHubDefinition> Hubs => models.SelectMany(m => m.Hubs).Distinct();
}
