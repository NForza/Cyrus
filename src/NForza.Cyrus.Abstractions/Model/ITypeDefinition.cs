namespace NForza.Cyrus.Abstractions.Model
{
    public interface ITypeDefinition
    {
        string Type { get; set; }
        bool IsCollection { get; set; }
        bool IsNullable { get; set; }
        ModelTypeDefinition[] SupportTypes { get; set; }
    }               
}