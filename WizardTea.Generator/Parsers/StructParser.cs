using System.Text;
using System.Xml.Linq;
using WizardTea.Generator.Injection;

namespace WizardTea.Generator.Parsers;

public class StructParser : BaseParser {
    private List<string> BlacklistedTypes { get; }
    private Dictionary<string, string> Data { get; } = [];

    public StructParser(XDocument xml) : base(xml) {
        BlacklistedTypes = ["string", "Vector3", "Vector2"];
    }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var structElements = root.Elements("struct").ToList();

        foreach (var structElem in structElements) {
            var structName = XmlHelper.GetRequiredAttributeValue(structElem, "name");
            var isGeneric = bool.TryParse(
                XmlHelper.GetOptionalAttributeValue(structElem, "generic"),
                out var result
            ) && result;

            if (BlacklistedTypes.Contains(structName) || structName.StartsWith("bhk") || structName.StartsWith("BS")) {
                continue;
            }

            var sb = new StringBuilder();
            var injector = InjectionRegistry.GetForStruct(structName) ?? new Injector();

            var structStartContext = new InjectionContext {
                StructName = structName,
                StructElement = structElem,
                CurrentSource = Prelude()
            };

            sb.AppendLine(injector.Execute(InjectionPoint.StructStart, structStartContext));
            sb.AppendLine(!isGeneric
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
                    sb.AppendLine("    " + before + " // injection: BeforeField");
                }

                var wasOverridden = false;
                fieldContext.CurrentSource = baseFieldCode;

                string fieldFinal;
                if (injector.HasAny(InjectionPoint.FieldOverride)) {
                    var overridden = injector.Execute(InjectionPoint.FieldOverride, fieldContext);
                    wasOverridden = !overridden.Equals(baseFieldCode, StringComparison.Ordinal);
                    fieldFinal = overridden;
                } else {
                    fieldFinal = baseFieldCode;
                }

                var finalLine = "    " + fieldFinal;
                if (wasOverridden) {
                    finalLine += " // injection: FieldOverride";
                }

                sb.AppendLine(finalLine);

                var after = injector.Execute(InjectionPoint.AfterField, fieldContext);
                if (!string.IsNullOrWhiteSpace(after) && after != fieldFinal) {
                    sb.AppendLine("    " + after + " // injection: AfterField");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"    public {structName}() {{ }}");
            sb.AppendLine();

            var structEndContext = new InjectionContext {
                StructName = structName,
                StructElement = structElem,
                CurrentSource = "",
            };

            var structEndCode = injector.Execute(InjectionPoint.StructEnd, structEndContext);
            if (!string.IsNullOrWhiteSpace(structEndCode)) {
                var indented = IndentLines(structEndCode, 4);
                sb.AppendLine(indented + " // injection: StructEnd");
            }

            sb.AppendLine("}");
            Data[structName] = sb.ToString();
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/STRUCT_{data.Key}.g.cs", data.Value);
        }
    }

    private static string IndentLines(string input, int spaceCount) {
        var indent = new string(' ', spaceCount);

        return string.Join(
            Environment.NewLine,
            input
                .Split(["\r\n", "\n"], StringSplitOptions.None)
                .Select(line => indent + line)
        );
    }
}