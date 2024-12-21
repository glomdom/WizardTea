using System.Text;
using WizardTea.Blocks;
using WizardTea.Blocks.NiMain;
using WizardTea.Core.Types;
using WizardTea.Internal;

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

    public string ReadIndexString(NifHeader header) {
        var index = ReadUInt32();

        return index == 0xFFFFFFFF ? string.Empty : header.Strings[index];
    }
    
    public T[] ReadArray<T>(int length, Func<T> reader) {
        if (length < 0) {
            throw new ArgumentOutOfRangeException(nameof(length), "length must be non-negative");
        }

        return ReadArray((uint)length, reader);
    }

    public T[] ReadArray<T>(uint length, Func<T> reader) {
        var result = new T[length];
        for (var i = 0; i < length; i++) {
            result[i] = reader();
        }
        
        return result;
    }
    
    public void ReadArrayInto<T>(ref T[] array, int length, Func<T> reader) {
        if (length < 0) {
            throw new ArgumentOutOfRangeException(nameof(length), "length must be non-negative");
        }

        ReadArrayInto(ref array, (uint)length, reader);
    }

    public void ReadArrayInto<T>(ref T[] array, uint length, Func<T> reader) {
        array = new T[length];

        for (var i = 0; i < length; i++) {
            array[i] = reader();
        }
    }

    public NiObject[] ParseBlocks(NifHeader header) {
        var blocks = new NiObject[header.NumBlocks];

        header.BlockTypeIndex.Each((typeIndex, i) => {
            Console.WriteLine($"Parsing {header.BlockTypes[typeIndex]}");

            blocks[i] = header.BlockTypes[typeIndex] switch {
                "NiNode" => new NiNode(this, header),
                "NiZBufferProperty" => new NiZBufferProperty(this, header),
                "NiVertexColorProperty" => new NiVertexColorProperty(this, header),
                "NiTriStrips" => new NiTriStrips(this, header),
                "NiMaterialProperty" => new NiMaterialProperty(this, header),
                "NiTexturingProperty" => new NiTexturingProperty(this, header),
                "NiSourceTexture" => new NiSourceTexture(this, header),
                "NiPersistentSrcTextureRendererData" => new NiPersistentSrcTextureRendererData(this, header),
                "NiTriStripsData" => new NiTriStripsData(this, header),
                _ => throw new NotImplementedException($"cannot parse {header.BlockTypes[typeIndex]}"),
            };

            return true;
        });

        return blocks;
    }
}