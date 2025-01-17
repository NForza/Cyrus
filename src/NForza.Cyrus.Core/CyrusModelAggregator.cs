using NForza.Cyrus.TypedIds;

namespace NForza.Cyrus
{
    internal class CyrusModelAggregator(ICyrusModel model1, ICyrusModel model2) : ICyrusModel
    {
        public IEnumerable<string> Strings => model1.Strings.Concat(model2.Strings).Distinct();
        public IEnumerable<string> Integers => model1.Integers.Concat(model2.Integers).Distinct();
        public IEnumerable<string> Events => model1.Events.Concat(model2.Events).Distinct();
        public IEnumerable<string> Commands => model1.Commands.Concat(model2.Commands).Distinct();
    }
}