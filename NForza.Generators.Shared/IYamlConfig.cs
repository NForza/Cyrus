using System.Collections.Generic;

namespace NForza.Generators
{
    public interface IYamlConfig<T>
    {
        T InitFrom(Dictionary<string, List<string>> config);
    }
}