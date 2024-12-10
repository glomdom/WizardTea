using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public abstract class NiTimeController : NiObject {
    public Ref<NiTimeController> NextController { get; set; }

    protected NiTimeController(NifStream stream) {
        NextController = new Ref<NiTimeController>(stream);
    }
}