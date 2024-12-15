using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

// TODO: Parse optionals
public class ShaderTexDesc {
    public bool HasMap { get; set; }

    public ShaderTexDesc(NifStream stream) {
        HasMap = stream.ReadBoolean();
    }
}