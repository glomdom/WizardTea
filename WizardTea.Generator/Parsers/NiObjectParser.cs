using System.Text;
using System.Xml.Linq;

namespace WizardTea.Generator.Parsers;

public class NiObjectParser : BaseParser {
    private Dictionary<string, string> Data { get; } = [];

    private List<string> BlacklistedModules { get; set; }

    public NiObjectParser(XDocument xml) : base(xml) {
        BlacklistedModules = ["BSHavok", "BSMain"];
    }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var niobjectElements = root.Elements("niobject");

        foreach (var niobjectElement in niobjectElements) {
            var name = XmlHelper.GetRequiredAttributeValue(niobjectElement, "name");

            var module = XmlHelper.GetOptionalAttributeValue(niobjectElement, "module");
            if (module is not null && BlacklistedModules.Contains(module)) continue;
            if (name.StartsWith("MdlMan")) continue; // we don't want divinity stuff. no clue why it isn't in its own module but meh

            var isAbstract = XmlHelper.GetOptionalAttributeValue(niobjectElement, "abstract") is not null;
            var inherit = XmlHelper.GetOptionalAttributeValue(niobjectElement, "inherit");

            var classType = isAbstract ? "abstract class" : "class";
            var baseClass = !string.IsNullOrWhiteSpace(inherit) ? $" : {inherit}" : "";

            var sb = new StringBuilder();
            sb.AppendLine(Prelude());
            sb.AppendLine($"public {classType} {name}{baseClass} {{");

            sb.AppendLine("}");
            Data.Add(name, sb.ToString());
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/NIOBJECT_{data.Key}.g.cs", data.Value);
        }
    }
}