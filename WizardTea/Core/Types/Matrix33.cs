namespace WizardTea.Core.Types;

public class Matrix33 {
    public float M11 { get; set; }
    public float M21 { get; set; }
    public float M31 { get; set; }
    public float M12 { get; set; }
    public float M22 { get; set; }
    public float M32 { get; set; }
    public float M13 { get; set; }
    public float M23 { get; set; }
    public float M33 { get; set; }
    
    public Matrix33(NifStream stream) {
        M11 = stream.ReadSingle();
        M21 = stream.ReadSingle();
        M31 = stream.ReadSingle();
        M12 = stream.ReadSingle();
        M22 = stream.ReadSingle();
        M32 = stream.ReadSingle();
        M13 = stream.ReadSingle();
        M23 = stream.ReadSingle();
        M33 = stream.ReadSingle();
    }
}