namespace WizardTea.Generator;

internal static class Utilities {
    internal static string IndentLines(string input, int spaceCount) {
        var indent = new string(' ', spaceCount);

        return string.Join(
            Environment.NewLine,
            input
                .Split(["\r\n", "\n"], StringSplitOptions.None)
                .Select(line => indent + line)
        );
    }
}