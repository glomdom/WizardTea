using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public abstract class NiAVObject : NiObjectNET {
    protected NiAVObject(NifStream stream, NifHeader header) : base(stream, header) {
        Console.WriteLine("Pretend we're parsing NiAVObject fields");
    }
}