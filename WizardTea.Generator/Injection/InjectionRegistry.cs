namespace WizardTea.Generator.Injection;

public static class InjectionRegistry {
    private static readonly Dictionary<string, Lazy<Injector>> _injectors = new() {
        ["BodyPartList"] = new Lazy<Injector>(() => {
            var injector = new Injector();
            injector.Register(
                InjectionPoint.StructEnd,
                ctx => "// the injector was here"
            );

            return injector;
        }),
    };

    public static Injector? GetForStruct(string structName) {
        return _injectors.TryGetValue(structName, out var injector) ? injector.Value : null;
    }
}