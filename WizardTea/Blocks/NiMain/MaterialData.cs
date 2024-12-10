﻿using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class MaterialData {
    public uint NumMaterials { get; set; }
    public string[] MaterialName { get; set; }
    public int[] MaterialExtraData { get; set; }
    public int ActiveMaterial { get; set; }
    public bool MaterialNeedsUpdate { get; set; }

    public MaterialData(NifStream stream, NifHeader header) {
        NumMaterials = stream.ReadUInt32();
        MaterialName = new string[NumMaterials];
        for (var i = 0; i < MaterialName.Length; i++) {
            MaterialName[i] = stream.ReadIndexString(header);
        }
        
        MaterialExtraData = new int[NumMaterials];
        for (var i = 0; i < MaterialExtraData.Length; i++) {
            MaterialExtraData[i] = stream.ReadInt32();
        }
        
        ActiveMaterial = stream.ReadInt32();
        MaterialNeedsUpdate = stream.ReadBoolean();
    }
}