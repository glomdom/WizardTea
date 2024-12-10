using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public abstract class NiDynamicEffect : NiAVObject {
    // TODO: Finish parsing
    
    protected NiDynamicEffect(NifStream stream, NifHeader header) : base(stream, header) { }
}