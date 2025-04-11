using System.Text;
using System.Xml.Linq;

namespace WizardTea.Generator.Parsers;

public class BitflagParser : BaseParser {
    private Dictionary<string, string> Data { get; } = [];

    public BitflagParser(XDocument xml) : base(xml) { }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var bitflagsElements = root.Elements("bitflags");

        foreach (var bitflagElement in bitflagsElements) {
            var bitflagStorage = XmlHelper.GetRequiredAttributeValue(bitflagElement, "storage");
            var bitflagName = XmlHelper.GetRequiredAttributeValue(bitflagElement, "name");
            var optionElements = bitflagElement.Elements("option");

            var sb = new StringBuilder();
            sb.AppendLine(Prelude());
            sb.AppendLine("[Flags]");
            sb.AppendLine($"public enum {bitflagName} : {bitflagStorage} {{");

            foreach (var optionElement in optionElements) {
                var name = XmlHelper.GetRequiredAttributeValue(optionElement, "name").Replace(" ", "_").ToUpper();
                var bit = XmlHelper.GetRequiredAttributeValue(optionElement, "bit");

                sb.AppendLine($"    {name} = {bit},");
            }

            sb.AppendLine("}");
            Data.Add(bitflagName, sb.ToString());
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/FLAGS_{data.Key}.g.cs", data.Value);
        }
    }
}