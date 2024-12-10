using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class NiNode : NiAVObject {
    public NiNode(NifStream stream, NifHeader header) : base(stream, header) {
        Console.WriteLine("Pretend we're parsing NiNode fields");
    }
}