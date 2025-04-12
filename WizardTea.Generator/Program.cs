using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using WizardTea.Generator.Injection;
using WizardTea.Generator.Parsers;
using static WizardTea.Generator.Injection.DefaultInjections;
using static WizardTea.Generator.Injection.InjectionBuilder;

namespace WizardTea.Generator {
    internal static class Program {
        private const string XmlUrl = "https://raw.githubusercontent.com/niftools/nifxml/refs/heads/develop/nif.xml";
        private const string XmlCachePath = "obj/WizardTeaData/cached.xml";
        private const string EtagCachePath = "obj/WizardTeaData/etag.txt";
        private const string GeneratedOutputPath = "Generated/";

        public static async Task Main() {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            Directory.CreateDirectory(Path.GetDirectoryName(XmlCachePath)!);
            Directory.CreateDirectory(GeneratedOutputPath);

            using var client = new HttpClient();
            var previousEtag = File.Exists(EtagCachePath) ? await File.ReadAllTextAsync(EtagCachePath) : null;

            if (!string.IsNullOrEmpty(previousEtag)) {
                client.DefaultRequestHeaders.IfNoneMatch.ParseAdd(previousEtag);
            }

            try {
                var response = await client.GetAsync(XmlUrl);

                if (response.StatusCode == System.Net.HttpStatusCode.NotModified) {
                    Log.Information("no xml updates (etags matched)");
                    if (!File.Exists(XmlCachePath)) return;

                    var cachedXml = await File.ReadAllTextAsync(XmlCachePath);
                    var cachedDoc = XDocument.Parse(cachedXml);

                    GenerateRawCode(cachedDoc);
                    RewriteFiles();

                    return;
                }

                Log.Information("cache doesnt exist, creating");

                if (!response.IsSuccessStatusCode) {
                    Log.Error("status is not success while fetching xml: {statusCode}", response.StatusCode);

                    return;
                }

                Log.Information("downloading new xml");

                var newXml = await response.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(XmlCachePath, newXml);

                Log.Information("downloaded xml");

                if (response.Headers.ETag != null) {
                    await File.WriteAllTextAsync(EtagCachePath, response.Headers.ETag.Tag);

                    Log.Information("cached new etag {}", response.Headers.ETag.Tag);
                }

                var doc = XDocument.Parse(newXml);
                GenerateRawCode(doc);
                RewriteFiles();
            } catch (HttpRequestException ex) {
                Log.Fatal("http request failed: {error}", ex.Message);
            } catch (Exception ex) {
                Log.Fatal("an error occurred: {error}", ex.Message);
            }
        }

        /// <summary>
        /// Generates basic C# types for every NIF type.
        /// Generated code from here is expected to be
        /// handed to the Roslyn Syntax Rewriter.
        /// </summary>
        /// <param name="xml">The XML to generate code from.</param>
        private static void GenerateRawCode(XDocument xml) {
            RegisterInjections();

            var enumParser = new EnumParser(xml);
            enumParser.Parse();
            enumParser.Generate();
            Log.Information("generated enums");

            var bitflagParser = new BitflagParser(xml);
            bitflagParser.Parse();
            bitflagParser.Generate();
            Log.Information("generated bitflags");

            var bitfieldParser = new BitfieldParser(xml);
            bitfieldParser.Parse();
            bitfieldParser.Generate();
            Log.Information("generated bitfields");

            var niobjectParser = new NiObjectParser(xml);
            niobjectParser.Parse();
            niobjectParser.Generate();
            Log.Information("generated niobjects");

            var structParser = new StructParser(xml);
            structParser.Parse();
            structParser.Generate();
            Log.Information("generated structs");
        }

        private static void RegisterInjections() {
            Log.Information("registering injections, might take some time");

            InjectionRegistry.Register(
                Use(NormbyteToByte),
                Use(HFloatToHalf),
                Use(BlockTypeIndexToUint),
                Use(Ulittle32ToUint),
                Use(FileVersionToInt),
                Use(FloatMaxTokenToValue),
                Use(XAxisTokenToValue),
                Use(Vec2OneTokenToValue),
                Use(EndianLittleToEndianType).For("Header"),
                Use(SystemVector3ToXYZ).For("ByteVector3"),
                Use(XYZToSystemVector3).For("ByteVector3"),
                Use(MIP_FMT_DEFAULTToMipMapFormat).For("FormatPrefs"),
                Use(ALPHA_DEFAULTToAlphaFormat).For("FormatPrefs"),
                Use(CPUToNxDeviceCode).For("NxCompartmentDescMap"),
                Use(SCT_RigidBodyToNxCompartmentType).For("NxCompartmentDescMap"),
                Use(WRAP_S_WRAP_TToTexClampMode).For("TexDesc"),
                Use(FILTER_TRILERPToTexFilterMode).For("TexDesc"),
                Use(Vector2ToTexCoord).For("TexCoord"),
                Use(NiControllerSequenceTextKeysToNonHide).For("NiControllerSequence"),
                Use(NiControllerSequenceAccumRootNameToNonHide).For("NiControllerSequence"),
                Use(NiParticleSystemDataToNonHide).For("NiParticleSystem")
            );

            Log.Information("registered injections");
        }

        private static void RewriteFiles() {
            var trees = new List<SyntaxTree>();
            var fileMap = new Dictionary<SyntaxTree, string>();

            foreach (var file in Directory.GetFiles(GeneratedOutputPath)) {
                // Log.Debug("parsing {fileName}", file);

                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                trees.Add(tree);
                fileMap.Add(tree, file);
            }

            // mscorlib stuff and some common system libs
            MetadataReference[] references = [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location)
            ];

            var compilation = CSharpCompilation.Create("GeneratedNIFObjects")
                .AddSyntaxTrees(trees)
                .AddReferences(references)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            
            Log.Debug("created compilation");
            
            foreach (var tree in trees) {
                var model = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();

                // Log.Debug("rewriting file {fileName}", fileMap[tree]);

                var rewriter = new Rewriter(model);
                var newRoot = rewriter.Visit(root);

                File.WriteAllText(fileMap[tree], newRoot.NormalizeWhitespace().ToFullString());
            }

            Log.Information("read methods created for all objects");
        }
    }
}