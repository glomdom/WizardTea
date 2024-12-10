using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public abstract class NiObjectNET : NiObject {
    public string Name { get; set; }

    protected NiObjectNET(NifStream stream, NifHeader header) {
        Name = stream.ReadIndexString(header);
        
        Console.WriteLine("Pretend we're parsing NiObjectNET fields");
    }
}