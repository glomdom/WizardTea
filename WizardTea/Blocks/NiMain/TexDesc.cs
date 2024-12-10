using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

// TODO: Parse
public class TexDesc {
    private ushort _bitfield;
    
    public Ref<NiSourceTexture> Source { get; set; }
    public TexturingMapFlags Flags {
        get => (TexturingMapFlags)(_bitfield & 0b_0011_1111_1111_1111);
        set => _bitfield = (ushort)((_bitfield & ~0b_0011_1111_1111_1111) | (ushort)value);
    }
    
    public bool HasTextureTransform { get; set; }

    public TexDesc(NifStream stream) {
        Source = new Ref<NiSourceTexture>(stream);
        _bitfield = stream.ReadUInt16();
        
        HasTextureTransform = stream.ReadBoolean();
    }
}