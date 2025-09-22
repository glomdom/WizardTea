using System.Text;
using System.Xml.Linq;
using Serilog;
using WizardTea.Generator.Injection;
using WizardTea.Generator.Metadata;

namespace WizardTea.Generator.Parsers;

public class StructParser : BaseParser {
    public int GeneratedCount { get; private set; }
    public int GeneratedFieldsCount { get; private set; }

    private List<string> BlacklistedTypes { get; }
    private List<string> BlacklistedModules { get; }
    private Dictionary<string, string> Data { get; } = [];

    public StructParser(XDocument xml) : base(xml) {
        BlacklistedTypes = ["string", "Vector3", "Vector2", "hkSubPartData"];
        BlacklistedModules = ["BSHavok", "BSMain", "BSAnimation"];
    }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element.");
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
                            ) &&
                            result;

            if (BlacklistedTypes.Contains(structName)) {
                continue;
            }

            var sb = new StringBuilder();
            var injector = InjectionRegistry.GetForItem(structName);

            var structStartContext = new InjectionContext {
                ItemName = structName,
                ItemElement = structElem,
                CurrentSource = Prelude(),
            };

            sb.AppendLine(injector.Execute(InjectionPoint.StructStart, structStartContext));
            sb.AppendLine(!isGeneric
                ? $"public struct {structName} {{"
                : $"public struct {structName}<T> {{"); // nif spec doesn't have more than 2 generics, so this is fine.

            sb.AppendLine("private NifReader _reader;");

            foreach (var structFieldElem in structElem.Elements("field")) {
                var rawFieldName = XmlHelper.GetRequiredAttributeValue(structFieldElem, "name");

                var fieldName = rawFieldName.Replace(" ", "_");
                var fieldType = XmlHelper.GetRequiredAttributeValue(structFieldElem, "type");
                var defaultValue = XmlHelper.GetOptionalAttributeValue(structFieldElem, "default");
                var template = XmlHelper.GetOptionalAttributeValue(structFieldElem, "template");
                var length = XmlHelper.GetOptionalAttributeValue(structFieldElem, "length");
                var since = XmlHelper.GetOptionalAttributeValue(structFieldElem, "since");
                var until = XmlHelper.GetOptionalAttributeValue(structFieldElem, "until");
                var condition = XmlHelper.GetOptionalAttributeValue(structFieldElem, "cond");
                var versionCondition = XmlHelper.GetOptionalAttributeValue(structFieldElem, "vercond");

                var metadata = new FieldMetadata();

                if (fieldType.StartsWith("bhk") || fieldType.StartsWith("BS")) {
                    continue;
                }

                if (fieldType == "#T#") {
                    fieldType = "T";
                }

                if (template is not null && template == "#T#") {
                    fieldType += "<T>";
                } else if (template is not null && template != "#T#") {
                    Log.Verbose("Generic of {template} applied for {fieldName} of type {fieldType}", template, fieldName, fieldType);
                    fieldType += $"<{template}>";
                }

                if (length is not null) {
                    fieldType += "[]";
                    metadata = metadata with { SizeIdentifier = length.Replace(" ", "_") };
                }

                if (since is not null) {
                    metadata = metadata with { VersionSince = since };
                }

                if (until is not null) {
                    metadata = metadata with { VersionUntil = until };
                }

                if (condition is not null) {
                    metadata = metadata with { Condition = condition };
                }

                if (versionCondition is not null) {
                    metadata = metadata with { VersionCondition = versionCondition };
                }

                if (defaultValue is not null) {
                    ParserHelper.RewriteValueBasedOnType(ref defaultValue, fieldType);
                }

                var baseFieldCode = defaultValue != null
                    ? $"public {fieldType} {fieldName} {{ get; set; }} = {defaultValue};"
                    : $"public {fieldType} {fieldName} {{ get; set; }}";

                GeneratedFieldsCount++;

                var fieldContext = new InjectionContext {
                    ItemName = structName,
                    ItemElement = structElem,
                    FieldElement = structFieldElem,
                    FieldName = fieldName,
                    FieldType = fieldType,
                    CurrentSource = "",
                };

                var before = injector.Execute(InjectionPoint.BeforeField, fieldContext);
                if (!string.IsNullOrWhiteSpace(before)) {
                    sb.AppendLine("    " + before);
                }

                fieldContext.CurrentSource = baseFieldCode;

                var fieldFinal = injector.HasAny(InjectionPoint.FieldOverride) ? injector.Execute(InjectionPoint.FieldOverride, fieldContext) : baseFieldCode;

                var finalLine = "    " + fieldFinal;
                sb.AppendLine(finalLine + metadata.ToComment());

                if (!injector.HasAny(InjectionPoint.AfterField)) continue;

                var after = injector.Execute(InjectionPoint.AfterField, fieldContext);
                if (!string.IsNullOrWhiteSpace(after) && after != fieldFinal) {
                    sb.AppendLine("    " + after);
                }
            }

            sb.AppendLine();
            sb.AppendLine($"    public {structName}(NifReader reader) {{ _reader = reader; }}");
            sb.AppendLine();

            var structEndContext = new InjectionContext {
                ItemName = structName,
                ItemElement = structElem,
                CurrentSource = "",
            };

            var structEndCode = injector.Execute(InjectionPoint.ItemEnd, structEndContext);
            if (!string.IsNullOrWhiteSpace(structEndCode)) {
                var indented = Utilities.IndentLines(structEndCode, 4);
                sb.AppendLine(indented);
            }

            sb.AppendLine("}");
            Data.Add(structName, sb.ToString());

            GeneratedCount++;
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/STRUCT_{data.Key}.g.cs", data.Value);
        }
    }
}