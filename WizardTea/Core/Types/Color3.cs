namespace WizardTea.Core.Types;

public class Color3 {
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }

    public Color3(NifStream stream) {
        R = stream.ReadSingle();
        G = stream.ReadSingle();
        B = stream.ReadSingle();
    }
}