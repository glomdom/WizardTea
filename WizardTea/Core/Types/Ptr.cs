namespace WizardTea.Core.Types;

public class Ptr<T> {
    public int Value { get; set; } // XXX: Figure out if it really is -1 or just doesnt get parsed!

    public Ptr(NifStream stream) {
        Value = stream.ReadInt32();
    }
    
    public bool HasPointer => Value != -1;
}