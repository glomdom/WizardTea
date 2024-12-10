using System.Diagnostics;
using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public abstract class NiAVObject : NiObjectNET {
    public ushort Flags { get; set; }
    public Vector3 Translation { get; set; }
    public Matrix33 Rotation { get; set; }
    public float Scale { get; set; }
    public uint NumProperties { get; set; }
    public Ref<NiProperty>[] Properties { get; set; }
    public Ref<NiCollisionObject> CollisionObject { get; set; }

    protected NiAVObject(NifStream stream, NifHeader header) : base(stream, header) {
        Flags = stream.ReadUInt16();
        Translation = new Vector3(stream);
        Rotation = new Matrix33(stream);
        Scale = stream.ReadSingle();
        
        NumProperties = stream.ReadUInt32();
        Properties = new Ref<NiProperty>[NumProperties];
        for (var i = 0; i < NumProperties; i++) {
            Properties[i] = new Ref<NiProperty>(stream);
        }
        
        CollisionObject = new Ref<NiCollisionObject>(stream);
    }
}