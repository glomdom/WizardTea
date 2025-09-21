using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace WizardTea.Generator.Metadata;

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public partial record FieldMetadata(
    string? SizeIdentifier = null,
    string? VersionSince = null,
    string? VersionUntil = null,
    string? Condition = null,
    string? VersionCondition = null
) {
    private static readonly Dictionary<string, string> TagMap = new() {
        { "si", nameof(SizeIdentifier) },
        { "vs", nameof(VersionSince) },
        { "vu", nameof(VersionUntil) },
        { "cd", nameof(Condition) },
        { "vc", nameof(VersionCondition) },
    };

    public override string ToString() {
        var parts = new List<string>();

        foreach (var (tag, propName) in TagMap) {
            var value = GetType().GetProperty(propName)?.GetValue(this);
            if (value is not null) parts.Add($"{tag}={value}");
        }

        return $"({string.Join(", ", parts)})";
    }

    public string ToComment() {
        if (GetType().GetProperties().All(p => p.GetValue(this) is null)) return "";

        var sb = new StringBuilder();
        sb.Append("// <nif");

        foreach (var (tag, propName) in TagMap) {
            var value = GetType().GetProperty(propName)?.GetValue(this);
            if (value is not null) sb.Append($":{tag}={value}");
        }

        sb.Append('>');

        return sb.ToString();
    }

    public static FieldMetadata FromString(string input) {
        if (string.IsNullOrWhiteSpace(input)) return new FieldMetadata();

        var matches = TagRegex().Matches(input);
        var values = new Dictionary<string, object?>();

        foreach (Match match in matches) {
            var key = match.Groups["key"].Value;
            var value = match.Groups["value"].Value;

            if (TagMap.TryGetValue(key, out var propName)) {
                values[propName] = value;
            }
        }

        var ctorParams = typeof(FieldMetadata)
            .GetConstructors().First()
            .GetParameters()
            .Select(p => values.GetValueOrDefault(p.Name!))
            .ToArray();

        return (FieldMetadata)Activator.CreateInstance(typeof(FieldMetadata), ctorParams)!;
    }

    [GeneratedRegex(":(?<key>[a-z]{2})=(?<value>[^:>]+)")]
    private static partial Regex TagRegex();
}