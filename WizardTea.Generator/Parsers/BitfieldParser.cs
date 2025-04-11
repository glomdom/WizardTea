using System.Text;
using System.Xml.Linq;

namespace WizardTea.Generator.Parsers;

public class BitfieldParser : BaseParser {
    private Dictionary<string, string> Data { get; } = [];

    public BitfieldParser(XDocument xml) : base(xml) { }

    public override void Parse() {
        var root = Xml.Root ?? throw new InvalidDataException("cache.xml missing root element");
        var bitfieldElements = root.Elements("bitfield");

        foreach (var bitfieldElement in bitfieldElements) {
            var storage = XmlHelper.GetRequiredAttributeValue(bitfieldElement, "storage");
            var name = XmlHelper.GetRequiredAttributeValue(bitfieldElement, "name");
            var memberElements = bitfieldElement.Elements("member");
            storage = GetCSType(storage);

            var is64Bit = storage == "ulong";

            var sb = new StringBuilder();
            sb.AppendLine(Prelude());
            sb.AppendLine($"public struct {name} {{");
            sb.AppendLine($"    private {storage} _value;");

            foreach (var member in memberElements) {
                var memberName = XmlHelper.GetRequiredAttributeValue(member, "name").Replace(" ", "_");
                var memberType = XmlHelper.GetRequiredAttributeValue(member, "type");
                var mask = XmlHelper.GetRequiredAttributeValue(member, "mask");
                var pos = int.Parse(XmlHelper.GetRequiredAttributeValue(member, "pos"));

                mask = is64Bit ? mask + "ul" : mask;
                
                sb.AppendLine();
                sb.AppendLine($"    public {memberType} {memberName} {{");

                if (memberType == "bool") {
                    sb.AppendLine($"        get => (_value & {mask}) != 0;");
                    sb.AppendLine($"        set => _value = ({storage})(value ? _value | {mask} : _value & ~{mask});");
                } else {
                    sb.AppendLine($"        get => ({memberType})((_value & {mask}) >> {pos});");
                    sb.AppendLine($"        set => _value = ({storage})((_value & ~{mask}) | ((({storage})value << {pos}) & {mask}));");
                }

                sb.AppendLine("    }");
            }

            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Implicit cast for this type. May result in loss of data, as we cast to {storage}.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public static implicit operator {name}(int value) => new {name} {{ _value = ({storage})value }};");

            sb.AppendLine("}");
            Data.Add(name, sb.ToString());
        }
    }

    public override void Generate() {
        foreach (var data in Data) {
            File.WriteAllText($"Generated/BITFIELD_{data.Key}.g.cs", data.Value);
        }
    }

    private string GetCSType(string type) {
        return type switch {
            "uint64" => "ulong",

            _ => type,
        };
    }
}