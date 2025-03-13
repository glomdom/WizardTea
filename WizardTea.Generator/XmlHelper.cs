using System.Xml.Linq;

namespace WizardTea.Generator;

public static class XmlHelper {
    public static XAttribute GetRequiredAttribute(XElement element, string attributeName) {
        return element.Attribute(attributeName) ?? throw new InvalidDataException($"{element.Name} element missing required attribute: {attributeName}");
    }

    public static string GetRequiredAttributeValue(XElement element, string attributeName) {
        return GetRequiredAttribute(element, attributeName).Value;
    }

    public static string? GetOptionalAttributeValue(XElement element, string attributeName) {
        return element.Attribute(attributeName)?.Value;
    }

    public static T GetRequiredAttributeValue<T>(XElement element, string attributeName) where T : IConvertible {
        var attribute = GetRequiredAttribute(element, attributeName);

        try {
            return (T)Convert.ChangeType(attribute.Value, typeof(T));
        } catch (Exception ex) {
            throw new InvalidDataException($"failed to convert attribute '{attributeName}' value '{attribute.Value}' to {typeof(T).Name}", ex);
        }
    }
}