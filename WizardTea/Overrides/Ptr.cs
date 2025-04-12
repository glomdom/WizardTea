namespace WizardTea;

public class Ptr<T> {
    private int Pointer { get; set; }

    private bool HasPointer => Pointer != -1;

    // TODO: Provide `NifFile` as parameter, to access blocks.
    public T? Get() {
        if (!HasPointer) return default;

        return default;
    }
}