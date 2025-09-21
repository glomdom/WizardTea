using System.Text;

namespace WizardTea;

public class NifReader : BinaryReader {
    public NifVersion Version { get; set; }
    
    public NifReader(Stream input, NifVersion version) : base(input) { }
    public NifReader(Stream input, Encoding encoding) : base(input, encoding) { }
    public NifReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }
}