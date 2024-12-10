using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class NiPalette : NiObject {
    public byte HasAlpha { get; set; }
    public uint NumEntries { get; set; }
    public ByteColor4[] Palette { get; set; }

    public NiPalette(NifStream stream) {
        HasAlpha = stream.ReadByte();
        NumEntries = stream.ReadUInt32();

        var entries = NumEntries == 16 ? 16 : 256;
        Palette = new ByteColor4[entries];
        for (var i = 0; i < entries; i++) {
            Palette[i] = new ByteColor4(stream);
        }
    }
}