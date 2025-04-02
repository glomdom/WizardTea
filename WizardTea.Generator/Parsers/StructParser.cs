using System.Text;
using System.Xml.Linq;
using WizardTea.Generator.Injection;

namespace WizardTea.Generator.Parsers;

public class StructParser : BaseParser {
    private List<string> BlacklistedTypes { get; }
    private Dictionary<string, string> Data { get; } = [];

    public StructParser(XDocument xml) : base(xml) {
        BlacklistedTypes = ["string"];
    }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var structElements = root.Elements("struct");

        foreach (var structElem in structElements) {
            var structName = XmlHelper.GetRequiredAttributeValue(structElem, "name");
            var isGeneric = bool.TryParse(
                XmlHelper.GetOptionalAttributeValue(structElem, "generic"),
                out var result
            ) && result;

            if (BlacklistedTypes.Contains(structName) || structName.StartsWith("bhk")) {
                continue;
            }

            var stringBuilder = new StringBuilder();
            var injector = InjectionRegistry.GetForStruct(structName) ?? new Injector();

            var structStartContext = new InjectionContext {
                StructName = structName,
                StructElement = structElem,
                CurrentSource = Prelude()
            };

            stringBuilder.AppendLine(injector.Execute(InjectionPoint.StructStart, structStartContext));
            stringBuilder.AppendLine(!isGeneric
                ? $"public struct {structName} {{"
                : $"public struct {structName}<T> {{"); // TODO: Support more than one type, depending on fields

            foreach (var structFieldElem in structElem.Elements("field")) {
                var fieldName = XmlHelper.GetRequiredAttributeValue(structFieldElem, "name").Replace(" ", "_");
                var fieldType = XmlHelper.GetRequiredAttributeValue(structFieldElem, "type");
                var defaultValue = XmlHelper.GetOptionalAttributeValue(structFieldElem, "default");

                if (fieldType == "#T#") {
                    fieldType = "T";
                }

                if (defaultValue is not null) {
                    ParserHelper.RewriteValueBasedOnType(ref defaultValue, fieldType);
                }

                var baseFieldCode = defaultValue != null
                    ? $"public {fieldType} {fieldName} {{ get; set; }} = {defaultValue};"
                    : $"public {fieldType} {fieldName} {{ get; set; }}";

                var fieldContext = new InjectionContext {
                    StructName = structName,
                    StructElement = structElem,
                    FieldElement = structFieldElem,
                    FieldName = fieldName,
                    FieldType = fieldType,
                    CurrentSource = ""
                };

                var before = injector.Execute(InjectionPoint.BeforeField, fieldContext);
                if (!string.IsNullOrWhiteSpace(before)) {
                    stringBuilder.AppendLine("    " + before);
                }

                fieldContext.CurrentSource = baseFieldCode;
                var fieldFinal = injector.Execute(InjectionPoint.FieldOverride, fieldContext);
                stringBuilder.AppendLine("    " + fieldFinal);

                var after = injector.Execute(InjectionPoint.AfterField, fieldContext);
                if (!string.IsNullOrWhiteSpace(after)) {
                    stringBuilder.AppendLine("    " + after);
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"    public {structName}() {{ }}");

            var structEndContext = new InjectionContext {
                StructName = structName,
                StructElement = structElem,
                CurrentSource = "",
            };

            var structEndCode = injector.Execute(InjectionPoint.StructEnd, structEndContext);
            if (!string.IsNullOrWhiteSpace(structEndCode)) {
                stringBuilder.AppendLine(structEndCode);
            }

            stringBuilder.AppendLine("}");
            Data[structName] = stringBuilder.ToString();
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/STRUCT_{data.Key}.g.cs", data.Value);
        }
    }

    private string ParseField(XElement fieldElem) {
        var name = XmlHelper.GetRequiredAttributeValue(fieldElem, "name").Replace(" ", "_");
        var type = XmlHelper.GetRequiredAttributeValue(fieldElem, "type");
        var defaultValue = XmlHelper.GetOptionalAttributeValue(fieldElem, "default");

        if (type == "#T#") {
            type = "T";
        }

        if (defaultValue is not null) {
            ParserHelper.RewriteValueBasedOnType(ref defaultValue, type);
        }

        return defaultValue != null
            ? $"public {type} {name} {{ get; set; }} = {defaultValue};"
            : $"public {type} {name} {{ get; set; }}";
    }
}