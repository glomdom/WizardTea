using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public class NiCollisionObject : NiObject {
    public required Ptr<NiAVObject> Target { get; set; }
}