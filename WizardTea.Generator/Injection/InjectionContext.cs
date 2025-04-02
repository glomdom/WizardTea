using System.Xml.Linq;

namespace WizardTea.Generator.Injection;

public class InjectionContext {
    public required string StructName { get; init; }
    public string? FieldName { get; init; }
    public string? FieldType { get; init; }
    public required string CurrentSource { get; set; }
    public required XElement StructElement { get; init; }
    public XElement? FieldElement { get; init; }
}