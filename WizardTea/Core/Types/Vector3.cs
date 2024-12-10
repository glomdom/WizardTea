namespace WizardTea.Core.Types;

public class Vector3 {
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public Vector3(NifStream stream) {
        X = stream.ReadSingle();
        Y = stream.ReadSingle();
        Z = stream.ReadSingle();
    }
}