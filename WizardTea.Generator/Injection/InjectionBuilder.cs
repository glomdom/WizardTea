using Serilog;

namespace WizardTea.Generator.Injection;

public static class InjectionBuilder {
    public static StructInjection Use(Action<Injector> apply) {
        Log.Verbose("Using {injectorName}", apply.Method.Name);

        return new StructInjection([], apply);
    }

    public static StructInjection For(this StructInjection injection, params string[] structs) {
        Log.Verbose("  For {structNames}", injection.Apply.Method.Name, structs);

        return new StructInjection(structs, injection.Apply);
    }
}