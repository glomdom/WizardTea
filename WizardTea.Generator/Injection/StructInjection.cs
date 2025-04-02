namespace WizardTea.Generator.Injection;

public sealed class StructInjection {
    public string[] StructNames { get; }
    public Action<Injector> Apply { get; }
    public bool IsGlobal => StructNames.Length == 0;

    public StructInjection(string[] structNames, Action<Injector> apply) {
        StructNames = structNames;
        Apply = apply;
    }
}