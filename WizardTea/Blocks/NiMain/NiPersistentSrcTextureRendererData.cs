﻿using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public class NiPersistentSrcTextureRendererData : NiPixelFormat {
    public Ref<NiPalette> Palette { get; set; }
    public uint NumMipmaps { get; set; }
    public uint BytesPerPixel { get; set; }
    public MipMap[] Mipmaps { get; set; }
    public uint NumPixels { get; set; }
    public uint PadNumPixels { get; set; }
    public uint NumFaces { get; set; }
    public PlatformID Platform { get; set; }
    public byte[] PixelData { get; set; }

    public NiPersistentSrcTextureRendererData(NifStream stream, NifHeader header) : base(stream, header) {
        Palette = new Ref<NiPalette>(stream);
        NumMipmaps = stream.ReadUInt32();
        BytesPerPixel = stream.ReadUInt32();
        
        Mipmaps = new MipMap[NumMipmaps];
        for (var i = 0; i < Mipmaps.Length; i++) {
            Mipmaps[i] = new MipMap(stream);
        }
        
        NumPixels = stream.ReadUInt32();
        PadNumPixels = stream.ReadUInt32();
        NumFaces = stream.ReadUInt32();
        Platform = (PlatformID)stream.ReadUInt32();
        
        PixelData = new byte[NumPixels * NumFaces];
        for (var i = 0; i < PixelData.Length; i++) {
            PixelData[i] = stream.ReadByte();
        }
    }
}