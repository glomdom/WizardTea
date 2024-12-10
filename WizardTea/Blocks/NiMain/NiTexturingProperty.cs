using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

// TODO: Parse optionals
public class NiTexturingProperty : NiProperty {
    private ushort _bitfield;

    public TexturingFlags Flags {
        get => (TexturingFlags)(_bitfield & 0b_0000_1111_1111_1111);
        set => _bitfield = (ushort)((_bitfield & ~0b_0000_1111_1111_1111) | (ushort)value);
    }

    public ApplyMode ApplyMode {
        get => (ApplyMode)(_bitfield & 0b_0000_1111_1111_0000);
        set => _bitfield = (ushort)((_bitfield & ~0b_0000_1111_1111_0000) | ((ushort)value << 4));
    }
    
    public uint TextureCount { get; set; }
    public bool HasBaseTexture { get; set; }
    public TexDesc BaseTexture { get; set; }
    
    public bool HasDarkTexture { get; set; }
    public bool HasDetailTexture { get; set; }
    public bool HasGlossTexture { get; set; }
    public bool HasGlowTexture { get; set; }
    public bool HasBumpMapTexture { get; set; }
    public bool HasNormalTexture { get; set; }
    public bool HasParallaxTexture { get; set; }
    public bool HasDecal0Texture { get; set; }
    
    public uint NumShaderTextures { get; set; }
    public ShaderTexDesc[] ShaderTextures { get; set; }

    public NiTexturingProperty(NifStream stream, NifHeader header) : base(stream, header) {
        _bitfield = stream.ReadUInt16();
        TextureCount = stream.ReadUInt32();
        
        HasBaseTexture = stream.ReadBoolean();
        BaseTexture = new TexDesc(stream);
        
        HasDarkTexture = stream.ReadBoolean();
        HasDetailTexture = stream.ReadBoolean();
        HasGlossTexture = stream.ReadBoolean();
        HasGlowTexture = stream.ReadBoolean();
        HasBumpMapTexture = stream.ReadBoolean();
        HasNormalTexture = stream.ReadBoolean();
        HasParallaxTexture = stream.ReadBoolean();
        HasDecal0Texture = stream.ReadBoolean();
        
        NumShaderTextures = stream.ReadUInt32();
        ShaderTextures = new ShaderTexDesc[NumShaderTextures];
        for (var i = 0; i < NumShaderTextures; i++) {
            ShaderTextures[i] = new ShaderTexDesc(stream);
        }
    }
}

[Flags]
public enum TexturingFlags : ushort {
    Multitexture = 0b_0000_0000_0000_0001,
    ApplyMode = 0b_0000_0000_0000_1110,
    DecalCount = 0b_0000_1111_1111_0000,
}

[Flags]
public enum TexturingMapFlags : ushort {
    TextureIndex = 0b_0000_0000_1111_1111,
    FilterMode = 0b_0000_1111_0000_0000,
    TexClampMode = 0b_0011_0000_0000_0000,
}

public enum TexFilterMode : uint {
    FILTER_NEAREST,
    FILTER_BILERP,
    FILTER_TRILERP,
    FILTER_NEAREST_MIPNEAREST,
    FILTER_NEAREST_MIPLERP,
    FILTER_BILERP_MIPNEAREST,
    FILTER_ANISOTROPIC,
}

public enum TexClampMode : uint {
    CLAMP_S_CLAMP_T,
    CLAMP_S_WRAP_T,
    WRAP_S_CLAMP_T,
    WRAP_S_WRAP_T,
}

public enum ApplyMode : uint {
    APPLY_REPLACE,
    APPLY_DECAL,
    APPLY_MODULATE,
    APPLY_HILIGHT,
    APPLY_HILIGHT2,
}