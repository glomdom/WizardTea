﻿using System.Text;
using WizardTea.Blocks;
using WizardTea.Blocks.NiMain;
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

        return header.Strings[index];
    }
    
    public NiObject[] ParseBlocks(NifHeader header) {
        var blocks = new NiObject[header.NumBlocks];
        
        header.BlockTypeIndex.Each((typeIndex, i) => {
            Console.WriteLine($"Parsing {header.BlockTypes[typeIndex]}");

            blocks[i] = header.BlockTypes[typeIndex] switch {
                "NiNode" => new NiNode(this, header),
                _ => throw new InvalidOperationException("unknown block type")
            };
        });

        return blocks;
    }
}