using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public abstract class NiGeometryData : NiObject {
    public int GroupID { get; set; }
    public ushort NumVertices { get; set; }
    public byte KeepFlags { get; set; }
    public byte CompressFlags { get; set; }
    public bool HasVertices { get; set; }
    public Vector3[] Vertices { get; set; }

    private ushort _bitfield;

    public ushort HasUVSets {
        get => (ushort)(_bitfield & (ushort)NiGeometryDataFlags.HasUVSets);
        set => _bitfield = (ushort)((_bitfield & ~(ushort)NiGeometryDataFlags.HasUVSets) |
                                    (ushort)(value & (ushort)NiGeometryDataFlags.HasUVSets));
    }

    public ushort HavokMaterial {
        get => (ushort)(_bitfield & (ushort)NiGeometryDataFlags.HavokMaterial);
        set => _bitfield = (ushort)((_bitfield & ~(ushort)NiGeometryDataFlags.HavokMaterial) |
                                    (ushort)(value & (ushort)NiGeometryDataFlags.HavokMaterial));
    }

    public NiNBTMethod NBTMethod {
        get => (NiNBTMethod)(_bitfield & (ushort)NiGeometryDataFlags.NBTMethod);
        set => _bitfield = (ushort)((_bitfield & ~(ushort)NiGeometryDataFlags.NBTMethod) |
                                    (ushort)((ushort)value & (ushort)NiGeometryDataFlags.NBTMethod));
    }

    public bool HasNormals { get; set; }

    public Vector3[] Normals { get; set; }

    // public Vector3[] Tangents { get; set; } // XXX: Figure out how these work.
    // public Vector3[] Bitangents { get; set; }
    public NiBound BoundingSphere { get; set; }
    public bool HasVertexColors { get; set; }
    public Vector3[] VertexColors { get; set; }
    public TexCoord[] UVSets { get; set; }
    public ConsistencyType ConsistencyFlags { get; set; }
    public Ref<AbstractAdditionalGeometryData> AdditionalData { get; set; }

    protected NiGeometryData(NifStream stream, NifHeader header) {
        GroupID = stream.ReadInt32();
        NumVertices = stream.ReadUInt16();
        KeepFlags = stream.ReadByte();
        CompressFlags = stream.ReadByte();
        
        HasVertices = stream.ReadBoolean();
        Vertices = HasVertices ? stream.ReadArray(NumVertices, () => new Vector3(stream)) : [];

        _bitfield = stream.ReadUInt16();

        HasNormals = stream.ReadBoolean();
        Normals = HasNormals ? stream.ReadArray(NumVertices, () => new Vector3(stream)) : [];
        // Tangents = new Vector3[HasVertices ? NumVertices : 0];
        // Bitangents = new Vector3[HasVertices ? NumVertices : 0];

        BoundingSphere = new NiBound(stream);
        HasVertexColors = stream.ReadBoolean();
        
        VertexColors = HasVertexColors ? stream.ReadArray(NumVertices, () => new Vector3(stream)) : [];
        UVSets = HasUVSets == 1 ? stream.ReadArray(NumVertices, () => new TexCoord(stream)) : [];
        
        ConsistencyFlags = (ConsistencyType)stream.ReadUInt16();
        AdditionalData = new Ref<AbstractAdditionalGeometryData>(stream);
    }
}

[Flags]
public enum NiGeometryDataFlags : ushort {
    HasUVSets = 0b_0000_0000_0011_1111,
    HavokMaterial = 0b_0000_1111_1100_0000,
    NBTMethod = 0b_1111_0000_0000_0000
}

public enum NiNBTMethod : uint {
    NBT_METHOD_NONE,
    NBT_METHOD_NDL,
    NBT_METHOD_MAX,
    NBT_METHOD_ATI
}

public enum ConsistencyType : ushort {
    CT_MUTABLE = 0x0000,
    CT_STATIC = 0x4000,
    CT_VOLATILE = 0x8000
}