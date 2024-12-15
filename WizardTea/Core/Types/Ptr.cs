using WizardTea.Blocks.NiMain;

namespace WizardTea.Core.Types;

public class Ptr<T> where T : NiObject {
    public int Value { get; set; }

    public Ptr(NifStream stream) {
        Value = stream.ReadInt32();
    }

    /// <summary>
    /// Gets the block being pointed at.
    /// </summary>
    /// <returns>`null` if there is nothing being pointed at.</returns>
    public T? GetPointing(NifFile file) {
        if (Value == -1) {
            return null;
        }
        
        return file.Blocks[Value] as T;
    }
    
    public bool HasPointer => Value != -1;
}