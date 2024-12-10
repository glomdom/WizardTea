using Dumpify;
using WizardTea.Core;

using var file = File.OpenRead(args[0]);
using var stream = new NifStream(file);

var nifFile = new NifFile(stream);
nifFile.Dump();