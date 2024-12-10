using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public class NiMaterialProperty : NiProperty {
    public Color3 AmbientColor { get; set; }
    public Color3 DiffuseColor { get; set; }
    public Color3 SpecularColor { get; set; }
    public Color3 EmissiveColor { get; set; }
    public float Glossiness { get; set; }
    public float Alpha { get; set; }

    public NiMaterialProperty(NifStream stream, NifHeader header) : base(stream, header) {
        AmbientColor = new Color3(stream);
        DiffuseColor = new Color3(stream);
        SpecularColor = new Color3(stream);
        EmissiveColor = new Color3(stream);
        Glossiness = stream.ReadSingle();
        Alpha = stream.ReadSingle();
    }
}