namespace WizardTea;

public class NifVersion {
    public NifVersionValue Value { get; private set; }
    public uint UserVersionValue { get; private set; }

    public NifVersion(NifVersionValue value, uint userVersionValue) {
        Value = value;
        UserVersionValue = userVersionValue;
    }
}