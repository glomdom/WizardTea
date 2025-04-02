using System.Xml.Linq;

namespace WizardTea.Generator.Injection;

public class InjectionContext {
    public string StructName { get; init; }
    public string? FieldName { get; init; }
    public string? FieldType { get; init; }
    public string CurrentSource { get; set; }
    public XElement StructElement { get; init; }
    public XElement?  FieldElement { get; init; }
}