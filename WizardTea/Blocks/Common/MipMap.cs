using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class MipMap {
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint Offset { get; set; }
    
    public MipMap(NifStream stream) {
        Width = stream.ReadUInt32();
        Height = stream.ReadUInt32();
        Offset = stream.ReadUInt32();
    }
}