﻿using WizardTea.Blocks;
using WizardTea.Blocks.NiMain;

namespace WizardTea.Core;

public class NifFile {
    public NifHeader Header { get; set; }
    public NiObject[] Blocks { get; set; }
    
    public NifFile(NifStream stream) {
        Header = new NifHeader(stream);
        Blocks = stream.ParseBlocks(Header);
    }

    public T? GetBlockOfName<T>(string name) where T : NiObjectNET {
        return Blocks.OfType<T>().FirstOrDefault(item => item.Name == name);
    }
}