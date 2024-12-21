namespace WizardTea.Core;

public class NifHeader {
    public string HeaderString { get; set; }
    public int Version { get; set; }
    public byte Endian { get; set; }
    public uint UserVersion { get; set; }
    public uint NumBlocks { get; set; }
    public ushort NumBlockTypes { get; set; }
    public string[] BlockTypes { get; set; }
    public ushort[] BlockTypeIndex { get; set; }
    public uint[] BlockSize { get; set; }
    public uint NumStrings { get; set; }
    public uint MaxStringLength { get; set; }
    public string[] Strings { get; set; }
    public uint NumGroups { get; set; }
    public uint[] Groups { get; set; }

    public NifHeader(NifStream stream) {
        HeaderString = stream.ReadLFTerminatedString();
        Version = stream.ReadInt32();
        Endian = stream.ReadByte();
        UserVersion = stream.ReadUInt32();
        NumBlocks = stream.ReadUInt32();
        NumBlockTypes = stream.ReadUInt16();
        
        BlockTypes = stream.ReadArray(NumBlockTypes, stream.ReadSizedString);
        BlockTypeIndex = stream.ReadArray(NumBlocks, stream.ReadUInt16);
        BlockSize = stream.ReadArray(NumBlocks, stream.ReadUInt32);
        
        NumStrings = stream.ReadUInt32();
        MaxStringLength = stream.ReadUInt32();
        Strings = stream.ReadArray(NumStrings, stream.ReadSizedString);
        
        NumGroups = stream.ReadUInt32();
        Groups = stream.ReadArray(NumGroups, stream.ReadUInt32);
    }
}