using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public abstract class NiGeometry : NiAVObject {
    public Ref<NiGeometryData> Data { get; set; }
    public Ref<NiSkinInstance> Skin { get; set; }
    public MaterialData MaterialData { get; set; }
    
    protected NiGeometry(NifStream stream, NifHeader header) : base(stream, header) {
        Data = new Ref<NiGeometryData>(stream);
        Skin = new Ref<NiSkinInstance>(stream);
        MaterialData = new MaterialData(stream, header);
    }
}