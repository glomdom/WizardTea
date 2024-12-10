using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public abstract class NiTriBasedGeom : NiGeometry {
    protected NiTriBasedGeom(NifStream stream, NifHeader header) : base(stream, header) { }
}