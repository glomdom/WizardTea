namespace WizardTea.Core.Types;

public class Ref<T> {
    public int Value { get; set; }

    public Ref(NifStream stream) {
        Value = stream.ReadInt32();
    }
    
    public bool HasReference => Value != -1;
}