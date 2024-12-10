using WizardTea.Core;

namespace WizardTea.Blocks.NiMain;

public class FilePath {
    public string Index { get; set; }

    public FilePath(NifStream stream, NifHeader header) {
        Index = stream.ReadIndexString(header);
    }
}