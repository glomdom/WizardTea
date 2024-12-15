using WizardTea.Blocks.NiMain;

namespace WizardTea.Core.Types;

public class Ref<T> where T : NiObject {
    public int Value { get; set; }

    public Ref(NifStream stream) {
        Value = stream.ReadInt32();
    }

    /// <summary>
    /// Gets the block being referenced.
    /// </summary>
    /// <returns>`null` if there is no reference.</returns>
    public T? GetReference(NifFile file) {
        if (Value == -1) {
            return null;
        }
        
        return file.Blocks[Value] as T;
    }
    
    public bool HasReference => Value != -1;
}