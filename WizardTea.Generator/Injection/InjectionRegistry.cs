namespace WizardTea.Generator.Injection;

public static class InjectionRegistry {
    private static readonly List<Action<Injector>> _globalInjections = new();
    private static readonly Dictionary<string, List<Action<Injector>>> _structInjections = new();

    public static void Register(params StructInjection[] injections) {
        foreach (var injection in injections) {
            if (injection.IsGlobal) {
                _globalInjections.Add(injection.Apply);
            } else {
                foreach (var structName in injection.StructNames) {
                    if (!_structInjections.ContainsKey(structName)) {
                        _structInjections[structName] = new List<Action<Injector>>();
                    }

                    _structInjections[structName].Add(injection.Apply);
                }
            }
        }
    }

    public static Injector GetForItem(string structName) {
        // Create a new Injector.
        var injector = new Injector();

        // Apply all global injections.
        foreach (var global in _globalInjections) {
            global(injector);
        }

        // Apply all injections specific for the given struct.
        if (_structInjections.TryGetValue(structName, out var actions)) {
            foreach (var action in actions) {
                action(injector);
            }
        }

        return injector;
    }

    public static int Count => _globalInjections.Count + _structInjections.Count;
}