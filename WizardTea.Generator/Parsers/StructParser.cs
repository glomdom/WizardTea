using System.Text;
using System.Xml.Linq;
using Serilog;
using WizardTea.Generator.Injection;

namespace WizardTea.Generator.Parsers;

public class StructParser : BaseParser {
    private List<string> BlacklistedTypes { get; }

    private List<string> BlacklistedModules { get; }
    private Dictionary<string, string> Data { get; } = [];

    public StructParser(XDocument xml) : base(xml) {
        BlacklistedTypes = ["string", "Vector3", "Vector2", "hkSubPartData"];
        BlacklistedModules = ["BSHavok", "BSMain", "BSAnimation"];
    }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var structElements = root.Elements("struct").ToList();

        foreach (var structElem in structElements) {
            var structName = XmlHelper.GetRequiredAttributeValue(structElem, "name");
            var module = XmlHelper.GetOptionalAttributeValue(structElem, "module");

            if (module is not null && BlacklistedModules.Contains(module)) {
                continue;
            }

            var isGeneric = bool.TryParse(
                XmlHelper.GetOptionalAttributeValue(structElem, "generic"),
                out var result
            ) && result;

            if (BlacklistedTypes.Contains(structName)) {
                continue;
            }

            var sb = new StringBuilder();
            var injector = InjectionRegistry.GetForItem(structName);

            var structStartContext = new InjectionContext {
                ItemName = structName,
                ItemElement = structElem,
                CurrentSource = Prelude()
            };

            sb.AppendLine(injector.Execute(InjectionPoint.StructStart, structStartContext));
            sb.AppendLine(!isGeneric
                ? $"public struct {structName} {{"
                : $"public struct {structName}<T> {{"); // nif spec doesn't have more than 2 generics, so this is fine.

            var seenFields = new List<string>();

            foreach (var structFieldElem in structElem.Elements("field")) {
                var rawFieldName = XmlHelper.GetRequiredAttributeValue(structFieldElem, "name");
                if (seenFields.Contains(rawFieldName)) {
                    continue;
                }

                seenFields.Add(rawFieldName);

                var fieldName = rawFieldName.Replace(" ", "_");
                var fieldType = XmlHelper.GetRequiredAttributeValue(structFieldElem, "type");
                var defaultValue = XmlHelper.GetOptionalAttributeValue(structFieldElem, "default");
                var template = XmlHelper.GetOptionalAttributeValue(structFieldElem, "template");
                var length = XmlHelper.GetOptionalAttributeValue(structFieldElem, "length");

                if (fieldType.StartsWith("bhk") || fieldType.StartsWith("BS")) {
                    continue;
                }

                if (fieldType == "#T#") {
                    fieldType = "T";
                }

                if (template is not null && template == "#T#") {
                    fieldType += "<T>";
                } else if (template is not null && template != "#T#") {
                    Log.Verbose("generic of {template} applied for {fieldName} of {fieldType}", template, fieldName, fieldType);
                    fieldType += $"<{template}>";
                }
                
                if (length is not null) {
                    fieldType += "[]";
                }

                if (defaultValue is not null) {
                    ParserHelper.RewriteValueBasedOnType(ref defaultValue, fieldType);
                }

                var baseFieldCode = defaultValue != null
                    ? $"public {fieldType} {fieldName} {{ get; set; }} = {defaultValue};"
                    : $"public {fieldType} {fieldName} {{ get; set; }}";

                var fieldContext = new InjectionContext {
                    ItemName = structName,
                    ItemElement = structElem,
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

                if (injector.HasAny(InjectionPoint.AfterField)) {
                    var after = injector.Execute(InjectionPoint.AfterField, fieldContext);
                    if (!string.IsNullOrWhiteSpace(after) && after != fieldFinal) {
                        sb.AppendLine("    " + after + " // injection: AfterField");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine($"    public {structName}() {{ }}");
            sb.AppendLine();

            var structEndContext = new InjectionContext {
                ItemName = structName,
                ItemElement = structElem,
                CurrentSource = "",
            };

            var structEndCode = injector.Execute(InjectionPoint.ItemEnd, structEndContext);
            if (!string.IsNullOrWhiteSpace(structEndCode)) {
                var indented = Utilities.IndentLines(structEndCode, 4);
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
}