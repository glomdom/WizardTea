using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public class NiBound {
    public Vector3 Center { get; set; }
    public float Radius { get; set; }

    public NiBound(NifStream stream) {
        Center = new Vector3(stream);
        Radius = stream.ReadSingle();
    }
}