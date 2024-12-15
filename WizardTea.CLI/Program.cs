using Dumpify;
using WizardTea.Core;

using var file = File.OpenRead(args[0]);
using var stream = new NifStream(file);

// keep this config if you don't want to spam your console with huge arrays of numbers
var config = new MembersConfig {
    MemberFilter = provider => provider.Name != "PixelData" && provider.Name != "Vertices" &&
                               provider.Name != "UVSets" && provider.Name != "Normals" &&
                               provider.Name != "Points"
};

var nifFile = new NifFile(stream);
nifFile.Dump(members: config);