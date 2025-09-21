namespace WizardTea;

public readonly record struct VersionRange(NifVersion? Since, NifVersion? Until) {
    public bool Matches(NifVersion version) {
        if (version < Since) return false;
        if (version > Until) return false;

        return true;
    }
}