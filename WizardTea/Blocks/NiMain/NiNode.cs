﻿using WizardTea.Core;
using WizardTea.Core.Types;

namespace WizardTea.Blocks.NiMain;

public class NiNode : NiAVObject {
    public uint NumChildren { get; set; }
    public Ref<NiAVObject>[] Children { get; set; }
    public uint NumEffects { get; set; }
    public Ref<NiDynamicEffect>[] Effects { get; set; }
    
    public NiNode(NifStream stream, NifHeader header) : base(stream, header) {
        NumChildren = stream.ReadUInt32();
        Children = stream.ReadArray(NumChildren, () => new Ref<NiAVObject>(stream));
        
        NumEffects = stream.ReadUInt32();
        Effects = stream.ReadArray(NumEffects, () => new Ref<NiDynamicEffect>(stream));
    }
}