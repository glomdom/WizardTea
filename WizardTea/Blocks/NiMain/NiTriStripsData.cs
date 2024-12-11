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
        StripLengths = new ushort[NumStrips];
        for (var i = 0; i < StripLengths.Length; i++) {
            StripLengths[i] = stream.ReadUInt16();
        }
        
        HasPoints = stream.ReadBoolean();
        Points = new ushort[StripLengths.Sum()];
        for (var i = 0; i < Points.Length; i++) {
            Points[i] = stream.ReadUInt16();
        }
    }
}