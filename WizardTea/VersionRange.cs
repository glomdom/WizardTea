namespace WizardTea;

public readonly record struct VersionRange(NifVersionValue? Since, NifVersionValue? Until) {
    public bool Matches(NifVersionValue version) {
        if (version < Since) return false;
        if (version > Until) return false;

        return true;
    }
}