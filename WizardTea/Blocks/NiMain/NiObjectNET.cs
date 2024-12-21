using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public abstract class NiObjectNET : NiObject {
    public string Name { get; set; }
    public uint NumExtraDataList { get; set; }
    public Ref<NiExtraData>[] ExtraDataList { get; set; }
    public Ref<NiTimeController> Controller { get; set; }

    protected NiObjectNET(NifStream stream, NifHeader header) {
        Name = stream.ReadIndexString(header);
        
        NumExtraDataList = stream.ReadUInt32();
        ExtraDataList = stream.ReadArray(NumExtraDataList, () => new Ref<NiExtraData>(stream));
        
        Controller = new Ref<NiTimeController>(stream);
    }
}