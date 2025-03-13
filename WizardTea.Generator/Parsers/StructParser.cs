using System.Text;
using System.Xml.Linq;

namespace WizardTea.Generator.Parsers;

public class StructParser : BaseParser {
    public List<string> BlacklistedTypes { get; }

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

            if (BlacklistedTypes.Contains(structName)) {
                continue;
            }

            // idk how to do these yet so we ignore them
            if (structName.StartsWith("bhk")) {
                continue;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(Prelude());
            stringBuilder.AppendLine(!isGeneric
                ? $"public struct {structName} {{"
                : $"public struct {structName}<T> {{"); // TODO: Support more than one type, depending on fields

            foreach (var structFieldElem in structElem.Elements("field")) {
                stringBuilder.AppendLine("    " + ParseField(structFieldElem));
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"    public {structName}() {{ }}");
            stringBuilder.AppendLine("}");

            Data.Add(structName, stringBuilder.ToString());
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