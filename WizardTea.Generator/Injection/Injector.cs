namespace WizardTea.Generator.Injection;

public delegate string InjectorFunction(InjectionContext context);

public sealed class Injector {
    private readonly Dictionary<InjectionPoint, List<InjectorFunction>> _injectors = [];
    private readonly Dictionary<string, Dictionary<InjectionPoint, List<InjectorFunction>>> _structInjectors = [];

    public void Register(InjectionPoint point, InjectorFunction func, string? structName = null) {
        if (structName == null) {
            if (!_injectors.ContainsKey(point)) {
                _injectors[point] = [];
            }

            _injectors[point].Add(func);
        }
        else {
            if (!_structInjectors.ContainsKey(structName)) {
                _structInjectors[structName] = [];
            }

            if (!_structInjectors[structName].ContainsKey(point)) {
                _structInjectors[structName][point] = [];
            }

            _structInjectors[structName][point].Add(func);
        }
    }


    public string Execute(InjectionPoint point, InjectionContext context) {
        var current = context.CurrentSource;

        if (_injectors.TryGetValue(point, out var globalFuncs)) {
            foreach (var func in globalFuncs) {
                context.CurrentSource = func(context);
            }
        }

        if (
            !_structInjectors.TryGetValue(context.StructName, out var perStruct) ||
            !perStruct.TryGetValue(point, out var structFuncs)
        ) {
            return context.CurrentSource;
        }

        {
            foreach (var func in structFuncs) {
                context.CurrentSource = func(context);
            }
        }

        return context.CurrentSource;
    }
}