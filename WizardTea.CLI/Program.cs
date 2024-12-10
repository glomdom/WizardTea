using Dumpify;
using WizardTea.Core;

using var file = File.OpenRead(args[0]);
using var stream = new NifStream(file);

var config = new MembersConfig() {
    MemberFilter = provider => {
        return provider.Name != "PixelData";
    }
};

var nifFile = new NifFile(stream);
nifFile.Dump(members: config);