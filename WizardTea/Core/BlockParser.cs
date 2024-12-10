using WizardTea.Blocks.NiMain;

namespace WizardTea.Core;

public static class BlockParser {
    public static void ParseBlocks(NifStream stream, NifHeader header) {
        foreach (var typeIndex in header.BlockTypeIndex) {
            Console.WriteLine($"Parsing {header.BlockTypes[typeIndex]}");

            switch (header.BlockTypes[typeIndex]) {
                case "NiNode": { // X: Test code.
                    var b = new NiNode(stream, header);
                    Console.WriteLine($"NiNode has indexed string name -> {b.Name}");

                    break;
                }
            }
        }
    }
}