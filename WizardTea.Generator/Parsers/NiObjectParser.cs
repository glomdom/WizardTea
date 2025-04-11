using System.Text;
using System.Xml.Linq;
using Serilog;
using WizardTea.Generator.Injection;

namespace WizardTea.Generator.Parsers;

public class NiObjectParser : BaseParser {
    private Dictionary<string, string> Data { get; } = [];

    private List<string> BlacklistedTypes { get; }

    public NiObjectParser(XDocument xml) : base(xml) {
        BlacklistedTypes = ["BSVertexDesc", "BSVertexDataSSE"];
    }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var niobjectElements = root.Elements("niobject");

        foreach (var niobjectElement in niobjectElements) {
            var className = XmlHelper.GetRequiredAttributeValue(niobjectElement, "name");
            var fields = niobjectElement.Elements("field");

            var module = XmlHelper.GetOptionalAttributeValue(niobjectElement, "module");
            if (module is not null && module.StartsWith("BS")) continue;
            if (className.StartsWith("MdlMan")) continue; // we don't want divinity stuff. no clue why it isn't in its own module but meh

            var isAbstract = XmlHelper.GetOptionalAttributeValue(niobjectElement, "abstract") is not null;
            var inherit = XmlHelper.GetOptionalAttributeValue(niobjectElement, "inherit");

            var classType = isAbstract ? "abstract class" : "class";
            var baseClass = !string.IsNullOrWhiteSpace(inherit) ? $" : {inherit}" : "";

            var sb = new StringBuilder();
            var injector = InjectionRegistry.GetForItem(className);

            sb.AppendLine(Prelude());
            sb.AppendLine($"public {classType} {className}{baseClass} {{");

            var seenFields = new List<string>();

            foreach (var field in fields) {
                var rawFieldName = XmlHelper.GetRequiredAttributeValue(field, "name");
                var fieldName = rawFieldName.RemoveAfter(':').Replace(" ", "_");

                if (seenFields.Contains(fieldName)) continue;

                seenFields.Add(fieldName);

                var fieldType = XmlHelper.GetRequiredAttributeValue(field, "type");
                if (BlacklistedTypes.Contains(fieldType)) continue;

                var template = XmlHelper.GetOptionalAttributeValue(field, "template");
                var length = XmlHelper.GetOptionalAttributeValue(field, "length");

                if (template is not null && fieldType != "Ref" && fieldType != "Ptr") {
                    // TODO: Support Ref & Ptr. Same as Struct.
                    Log.Verbose("generic of {template} applied for {fieldName} of {fieldType}", template, fieldName, fieldType);
                    fieldType += $"<{template}>";
                }

                var baseFieldCode = $"public {fieldType} {fieldName} {{ get; set; }}";

                var fieldContext = new InjectionContext {
                    ItemName = className,
                    ItemElement = niobjectElement,
                    FieldElement = field,
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

                // TODO: Implement #ARG# passing in. Very important for multi-functional classes.
            }

            sb.AppendLine();
            sb.AppendLine(string.IsNullOrEmpty(inherit) ? $"    public {className}() {{ }}" : $"    public {className}() : base() {{ }}");
            sb.AppendLine();

            var classEndContext = new InjectionContext {
                ItemName = className,
                ItemElement = niobjectElement,
                CurrentSource = "",
            };

            var classEndCode = injector.Execute(InjectionPoint.ItemEnd, classEndContext);
            if (!string.IsNullOrWhiteSpace(classEndCode)) {
                var indented = Utilities.IndentLines(classEndCode, 4);
                sb.AppendLine(indented + " // injection: StructEnd");
            }

            sb.AppendLine("}");
            Data.Add(className, sb.ToString());
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/NIOBJECT_{data.Key}.g.cs", data.Value);
        }
    }
}