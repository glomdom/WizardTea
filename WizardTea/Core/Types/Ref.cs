using WizardTea.Blocks.NiMain;

namespace WizardTea.Core.Types;

public class Ref<T> where T : NiObject {
    public int Value { get; set; }

    public Ref(NifStream stream) {
        Value = stream.ReadInt32();
    }

    public T GetReference(NifFile file) {
        return (T)file.Blocks[Value];
    }
    
    public bool HasReference => Value != -1;
}