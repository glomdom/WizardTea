namespace WizardTea;

public static class VersionHelpers {
    public static bool Matches(NifVersion current, VersionRange range) => range.Matches(current);

    public static NifVersion ParseVersion(string s) {
        var parts = s.Split('.').Select(int.Parse).ToArray();
        var value = (uint)(parts[0] << 24 | parts[1] << 16 | parts[2] << 8 | parts[3]);

        return (NifVersion)value;
    }
}