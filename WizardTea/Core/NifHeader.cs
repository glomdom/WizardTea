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

    public NifHeader(NifStream stream) {
        HeaderString = stream.ReadLFTerminatedString();
        Version = stream.ReadInt32();
        Endian = stream.ReadByte();
        UserVersion = stream.ReadUInt32();
        NumBlocks = stream.ReadUInt32();
        NumBlockTypes = stream.ReadUInt16();
        
        BlockTypes = new string[NumBlocks];
        for (var i = 0; i < BlockTypes.Length; i++) {
            BlockTypes[i] = stream.ReadSizedString();
        }
        
        BlockTypeIndex = new ushort[NumBlocks];
        for (var i = 0; i < BlockTypes.Length; i++) {
            BlockTypeIndex[i] = stream.ReadUInt16();
        }
    }
}