using System.Text;
using System.Xml.Linq;

namespace WizardTea.Generator.Parsers;

public class EnumParser : BaseParser {
    private Dictionary<string, string> Data { get; } = [];

    public EnumParser(XDocument xml) : base(xml) { }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var enumElements = root.Elements("enum");

        foreach (var enumElement in enumElements) {
            var enumStorage = XmlHelper.GetRequiredAttributeValue(enumElement, "storage");
            var enumName = XmlHelper.GetRequiredAttributeValue(enumElement, "name");
            var optionElements = enumElement.Elements("option");
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(Prelude());
            stringBuilder.AppendLine($"public enum {enumName} : {enumStorage} {{");

            foreach (var optionElement in optionElements) {
                var name = XmlHelper.GetRequiredAttributeValue(optionElement, "name").Replace(" ", "_").ToUpper();
                var value = XmlHelper.GetRequiredAttributeValue(optionElement, "value");

                stringBuilder.AppendLine($"    {name} = {value},");
            }

            stringBuilder.AppendLine("}");
            Data.Add(enumName, stringBuilder.ToString());
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/ENUM_{data.Key}.g.cs", data.Value);
        }
    }
}