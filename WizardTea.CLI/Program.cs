// TODO: MAKE A SUPER COOL VISUALIZER LIKE JSONCRACK OR SOMETHING

using Dumpify;
using WizardTea.Core;

using var file = File.OpenRead(args[0]);
using var stream = new NifStream(file);

var config = new MembersConfig {
    // keep this config if you don't want to spam your console with huge arrays of numbers
    MemberFilter = provider => provider.Name != "PixelData" && provider.Name != "Vertices" &&
                               provider.Name != "UVSets" && provider.Name != "Normals"
};

var nifFile = new NifFile(stream);
nifFile.Dump(members: config);