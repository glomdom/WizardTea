using WizardTea.Core;
using WizardTea.Internal;

namespace WizardTea.Blocks.NiMain;

public class NiTriStripsData : NiTriBasedGeomData {
    public ushort NumStrips { get; set; }
    public ushort[] StripLengths { get; set; }
    public bool HasPoints { get; set; }
    public ushort[] Points { get; set; }
    
    public NiTriStripsData(NifStream stream, NifHeader header) : base(stream, header) {
        NumStrips = stream.ReadUInt16();
        StripLengths = stream.ReadArray(NumStrips, stream.ReadUInt16);
        
        HasPoints = stream.ReadBoolean();
        Points = stream.ReadArray(StripLengths.Sum(), stream.ReadUInt16);
    }
}