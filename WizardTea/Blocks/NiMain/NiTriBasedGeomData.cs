using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public abstract class NiTriBasedGeomData : NiGeometryData {
    public ushort NumTriangles { get; set; }
    
    protected NiTriBasedGeomData(NifStream stream, NifHeader header) : base(stream, header) {
        NumTriangles = stream.ReadUInt16();
    }
}