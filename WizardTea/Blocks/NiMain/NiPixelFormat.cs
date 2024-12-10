using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

// TODO: Parse
public abstract class NiPixelFormat : NiObject {
    public PixelFormat PixelFormat { get; set; }
    public byte BitsPerPixel { get; set; }
    public uint RendererHint { get; set; }
    public uint ExtraData { get; set; }
    public byte Flags { get; set; }
    public PixelTiling Tiling { get; set; }
    public PixelFormatComponent[] Channels { get; set; }
    
    public NiPixelFormat(NifStream stream, NifHeader header) {
        PixelFormat = (PixelFormat)stream.ReadUInt32();
        BitsPerPixel = stream.ReadByte();
        RendererHint = stream.ReadUInt32();
        ExtraData = stream.ReadUInt32();
        Flags = stream.ReadByte();
        Tiling = (PixelTiling)stream.ReadUInt32();

        Channels = new PixelFormatComponent[4];
        for (var i = 0; i < Channels.Length; i++) {
            Channels[i] = new PixelFormatComponent(stream);
        }
    }
}