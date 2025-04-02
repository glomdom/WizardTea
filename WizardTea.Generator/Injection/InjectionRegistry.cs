namespace WizardTea.Generator.Injection;

public static class InjectionRegistry {
    private static readonly Dictionary<string, Lazy<Injector>> _structInjectors = [];
    private static readonly List<StructInjection> _globalInjectors = [];

    public static void Register(params StructInjection[] injections) {
        foreach (var injection in injections) {
            if (injection.IsGlobal) {
                _globalInjectors.Add(injection);

                continue;
            }
            
            foreach (var structName in injection.StructNames) {
                if (!_structInjectors.TryGetValue(structName, out var oldLazy)) {
                    _structInjectors[structName] = new Lazy<Injector>(() => {
                        var injector = new Injector();
                        injection.Apply(injector);

                        return injector;
                    });
                } else {
                    _structInjectors[structName] = new Lazy<Injector>(() => {
                        var injector = oldLazy.Value;
                        injection.Apply(injector);

                        return injector;
                    });
                }
            }
        }
    }

    public static Injector? GetForStruct(string structName) {
        var structInjector = _structInjectors.TryGetValue(structName, out var lazy) ? lazy.Value : null;

        if (_globalInjectors.Count == 0) return structInjector;

        var globalInjector = new Injector();
        foreach (var global in _globalInjectors) {
            global.Apply(globalInjector);
        }

        return structInjector == null ? globalInjector : Injector.Merge(globalInjector, structInjector);
    }
}