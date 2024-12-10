using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class NiVertexColorProperty : NiProperty {
    private ushort _bitfield;

    public VertexColorFlags Flags {
        get => (VertexColorFlags)(_bitfield & 0b_0000_0000_0111_1111);
        set => _bitfield = (ushort)((_bitfield & ~0b_0000_0000_0111_1111) | (ushort)value);
    }
    
    public LightningMode LightingMode {
        get => (LightningMode)((_bitfield >> 3) & 0b_1);
        set => _bitfield = (ushort)((_bitfield & ~0b_0000_0000_0000_1000) | ((ushort)value << 3));
    }
    
    public SourceVertexMode SourceVertexMode {
        get => (SourceVertexMode)((_bitfield >> 4) & 0b_0011);
        set => _bitfield = (ushort)((_bitfield & ~0b_0000_0000_0011_0000) | ((ushort)value << 4));
    }
    
    public NiVertexColorProperty(NifStream stream, NifHeader header) : base(stream, header) {
        _bitfield = stream.ReadUInt16();
    }
}

[Flags]
public enum VertexColorFlags : ushort {
    ColorMode = 0b_0000_0000_0000_0111,
    LightingMode = 0b_0000_0000_0001_0000,
    SourceVertexMode = 0b_0000_0000_0110_0000,
}

public enum SourceVertexMode : uint {
    VERT_MODE_SRC_IGNORE = 0,
    VERT_MODE_SRC_EMISSIVE = 1,
    VERT_MODE_SRC_AMB_DIF = 2
}

public enum LightningMode : uint {
    LIGHT_MODE_EMISSIVE = 0,
    LIGHT_MODE_EMI_AMB_DIF = 1
}