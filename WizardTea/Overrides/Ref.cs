namespace WizardTea;

public class Ref<T> {
    private int Reference { get; set; }

    private bool HasReference => Reference != -1;

    // TODO: Provide `NifFile` as parameter, to access blocks.
    public T? Get() {
        if (!HasReference) return default;

        return default;
    }
}