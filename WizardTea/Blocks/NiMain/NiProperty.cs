using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public abstract class NiProperty : NiObjectNET {
    protected NiProperty(NifStream stream, NifHeader header) : base(stream, header) { }
}