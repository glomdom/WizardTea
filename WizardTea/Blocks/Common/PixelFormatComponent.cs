using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class PixelFormatComponent {
    public PixelComponent Type { get; set; }
    public PixelRepresentation Convention { get; set; }
    public byte BitsPerChannel { get; set; }
    public bool IsSigned { get; set; }
    
    public PixelFormatComponent(NifStream stream) {
        Type = (PixelComponent)stream.ReadUInt32();
        Convention = (PixelRepresentation)stream.ReadUInt32();
        BitsPerChannel = stream.ReadByte();
        IsSigned = stream.ReadBoolean();
    }
}