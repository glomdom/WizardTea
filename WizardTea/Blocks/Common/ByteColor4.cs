using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class ByteColor4 {
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    public ByteColor4(NifStream stream) {
        R = stream.ReadByte();
        G = stream.ReadByte();
        B = stream.ReadByte();
        A = stream.ReadByte();
    }
}