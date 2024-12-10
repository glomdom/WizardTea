using WizardTea.Core;

using var file = File.OpenRead(args[0]);
using var stream = new NifStream(file);

var header = new NifHeader(stream);

Console.WriteLine(header.HeaderString);
Console.WriteLine($"Endian -> {header.Endian}");
Console.WriteLine($"User Version -> {header.UserVersion}");
Console.WriteLine($"Block Count -> {header.NumBlocks}");
Console.WriteLine($"String Count -> {header.NumStrings}");
Console.WriteLine($"Group Count -> {header.NumGroups}");

Console.WriteLine("BlockTypes [");
foreach (var blockType in header.BlockTypes) {
    Console.WriteLine("  " + blockType);
}
Console.WriteLine("]");

// X: Resolve block type for each index, would help with debugging. (?)
Console.WriteLine("BlockTypeIndex [");
foreach (var index in header.BlockTypeIndex) {
    Console.WriteLine("  " + index);
}
Console.WriteLine("]");

Console.WriteLine("BlockSizes [");
foreach (var size in header.BlockSize) {
    Console.WriteLine("  " + size);
}
Console.WriteLine("]");


Console.WriteLine("Strings [");
foreach (var str in header.Strings) {
    Console.WriteLine("  " + str);
}
Console.WriteLine("]");

Console.WriteLine("Groups [");
foreach (var group in header.Groups) {
    Console.WriteLine("  " + group);
}
Console.WriteLine("]");

stream.ParseBlocks(header);