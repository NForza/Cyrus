using System.Collections;
using System.Collections.Generic;

namespace NForza.Cyrus.TypescriptGenerate.Model;

public interface ITypeDefinition
{
    string Type { get; set; }
    bool IsCollection { get; set; }
    bool IsNullable { get; set; }
    IEnumerable<ITypeDefinition> SupportTypes { get; set; }
}