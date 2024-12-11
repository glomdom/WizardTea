using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class TexCoord {
    public float U { get; set; }
    public float V { get; set; }
    
    public TexCoord(NifStream stream) {
        U = stream.ReadSingle();
        V = stream.ReadSingle();
    }
}