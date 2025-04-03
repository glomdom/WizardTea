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
        } else {
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
        // For FieldOverride, chain modifications instead of accumulating separate outputs.
        if (point == InjectionPoint.FieldOverride) {
            string result = context.CurrentSource;
            // Apply global injections in order.
            if (_injectors.TryGetValue(point, out var globalFuncs)) {
                foreach (var func in globalFuncs) {
                    // Create a fresh context with the current result.
                    var localContext = new InjectionContext {
                        StructName = context.StructName,
                        FieldName = context.FieldName,
                        FieldType = context.FieldType,
                        StructElement = context.StructElement,
                        FieldElement = context.FieldElement,
                        CurrentSource = result
                    };

                    result = func(localContext);
                }
            }

            // Apply struct-specific injections in order.
            if (_structInjectors.TryGetValue(context.StructName, out var perStruct) &&
                perStruct.TryGetValue(point, out var structFuncs)) {
                foreach (var func in structFuncs) {
                    var localContext = new InjectionContext {
                        StructName = context.StructName,
                        FieldName = context.FieldName,
                        FieldType = context.FieldType,
                        StructElement = context.StructElement,
                        FieldElement = context.FieldElement,
                        CurrentSource = result
                    };

                    result = func(localContext);
                }
            }

            return result;
        } else {
            // For other injection points, accumulate all outputs.
            var outputs = new List<string>();
            if (!string.IsNullOrEmpty(context.CurrentSource)) {
                outputs.Add(context.CurrentSource);
            }

            if (_injectors.TryGetValue(point, out var globalFuncs)) {
                foreach (var func in globalFuncs) {
                    var result = func(context);
                    if (!string.IsNullOrEmpty(result)) {
                        outputs.Add(result);
                    }
                }
            }

            if (_structInjectors.TryGetValue(context.StructName, out var perStruct) &&
                perStruct.TryGetValue(point, out var structFuncs)) {
                foreach (var func in structFuncs) {
                    var result = func(context);
                    if (!string.IsNullOrEmpty(result)) {
                        outputs.Add(result);
                    }
                }
            }

            return string.Join(Environment.NewLine, outputs);
        }
    }

    public bool HasAny(InjectionPoint point) {
        var hasGlobal = _injectors.TryGetValue(point, out var globalList) && globalList.Count > 0;
        var hasStructSpecific = _structInjectors.Values
            .Any(dict => dict.TryGetValue(point, out var funcs) && funcs.Count > 0);

        return hasGlobal || hasStructSpecific;
    }

    public static Injector Merge(params Injector[] injectors) {
        var merged = new Injector();

        foreach (var injector in injectors) {
            foreach (var (point, value) in injector.GetAllInjectors()) {
                foreach (var func in value) {
                    merged.Register(point, func);
                }
            }
        }

        return merged;
    }

    private Dictionary<InjectionPoint, List<InjectorFunction>> GetAllInjectors() {
        return _injectors;
    }
}