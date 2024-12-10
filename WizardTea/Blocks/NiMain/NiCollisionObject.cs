using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public class NiCollisionObject : NiObject {
    public Ptr<NiAVObject> Target { get; set; }
}