using WizardTea.Core;

using var file = File.OpenRead(args[0]);
using var stream = new NifStream(file);

var header = new NifHeader(stream);

Console.WriteLine(header.HeaderString);
Console.WriteLine("0x" + header.Version.ToString("X"));
Console.WriteLine(header.Endian);
Console.WriteLine(header.UserVersion);

foreach (var blockType in header.BlockTypes) {
    Console.WriteLine(blockType);
}