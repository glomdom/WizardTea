using System.Text;

namespace WizardTea.Core;

public class NifStream : BinaryReader {
    public NifStream(Stream input) : base(input) { }
    public NifStream(Stream input, Encoding encoding) : base(input, encoding) { }
    public NifStream(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

    public string ReadLFTerminatedString() {
        var sb = new StringBuilder();

        char c;
        while ((c = ReadChar()) != 0x0A) {
            sb.Append(c);
        }
        
        return sb.ToString();
    }

    public string ReadSizedString() {
        var size = ReadUInt32();
        var bytes = ReadBytes((int)size);
        
        return Encoding.ASCII.GetString(bytes);
    } 

    internal int GetVersionFromHeaderString(string headerString) {
        var last = headerString.Trim().Split()[^1];
        var parts = last.Split('.').Select(int.Parse).ToArray();

        return parts[0] << 24 | parts[1] << 16 | parts[2] << 8 | parts[3];
    }
}