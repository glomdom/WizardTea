using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class FormatPrefs {
    public PixelLayout PixelLayout { get; set; }
    public MipMapFormat UseMipMaps { get; set; }
    public AlphaFormat AlphaFormat { get; set; }

    public FormatPrefs(NifStream stream) {
        PixelLayout = (PixelLayout)stream.ReadUInt32();
        UseMipMaps = (MipMapFormat)stream.ReadUInt32();
        AlphaFormat = (AlphaFormat)stream.ReadUInt32();
    }
}