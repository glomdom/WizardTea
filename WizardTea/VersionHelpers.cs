namespace WizardTea;

public static class VersionHelpers {
    public static bool Matches(NifVersionValue current, VersionRange range) => range.Matches(current);

    // TODO: Expand and parse version conditions.
    public static bool Matches(string condition, NifReader reader) {
        return false;
    }

    public static NifVersionValue ParseVersion(string s) {
        var parts = s.Split('.').Select(int.Parse).ToArray();
        var value = (uint)(parts[0] << 24 | parts[1] << 16 | parts[2] << 8 | parts[3]);

        return (NifVersionValue)value;
    }
}