using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public abstract class NiTexture : NiObjectNET {
    protected NiTexture(NifStream stream, NifHeader header) : base(stream, header) { }
}