using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public class NiSourceTexture : NiTexture {
    public byte UseExternal { get; set; }
    public FilePath FileName { get; set; }
    public Ref<NiPixelFormat> PixelData { get; set; }
    public FormatPrefs FormatPrefs { get; set; }
    public byte IsStatic { get; set; }
    public bool DirectRender { get; set; }
    public bool PersistRenderData { get; set; }

    public NiSourceTexture(NifStream stream, NifHeader header) : base(stream, header) {
        UseExternal = stream.ReadByte();
        FileName = new FilePath(stream, header);
        PixelData = new Ref<NiPixelFormat>(stream);
        FormatPrefs = new FormatPrefs(stream);
        IsStatic = stream.ReadByte();
        DirectRender = stream.ReadBoolean();
        PersistRenderData = stream.ReadBoolean();
    }
}