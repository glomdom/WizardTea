using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using WizardTea.Generator.Injection;
using WizardTea.Generator.Parsers;
using static WizardTea.Generator.Injection.DefaultInjections;
using static WizardTea.Generator.Injection.InjectionBuilder;

namespace WizardTea.Generator;

internal static class Program {
    private const string XmlUrl = "https://raw.githubusercontent.com/niftools/nifxml/refs/heads/develop/nif.xml";
    private const string XmlCachePath = "obj/WizardTeaData/cached.xml";
    private const string EtagCachePath = "obj/WizardTeaData/etag.txt";
    private const string GeneratedOutputPath = "Generated/";

    public static async Task Main() {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
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

            Log.Information("Fetching nif.xml from {url}", XmlUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.NotModified) {
                Log.Debug("No xml updates (etags matched)");
                if (!File.Exists(XmlCachePath)) return;

                var cachedXml = await File.ReadAllTextAsync(XmlCachePath);
                var cachedDoc = XDocument.Parse(cachedXml);

                GenerateRawCode(cachedDoc);
                RewriteFiles();

                return;
            }

            Log.Information("Cache doesnt exist, creating");

            if (!response.IsSuccessStatusCode) {
                Log.Error("Status is not success while fetching xml: {statusCode}", response.StatusCode);

                return;
            }

            Log.Information("Downloading new xml");

            var newXml = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(XmlCachePath, newXml);

            if (response.Headers.ETag != null) {
                await File.WriteAllTextAsync(EtagCachePath, response.Headers.ETag.Tag);

                Log.Information("Cached new etag {etag}", response.Headers.ETag.Tag);
            }

            var doc = XDocument.Parse(newXml);
            GenerateRawCode(doc);
            RewriteFiles();
        } catch (HttpRequestException ex) {
            Log.Fatal("Http request failed: {error}", ex.Message);
        } catch (Exception ex) {
            Log.Fatal("An error occurred: {error}", ex.Message);
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

        var enumWatch = Stopwatch.StartNew();
        var enumParser = new EnumParser(xml);
        using (UsingStopwatch(enumWatch)) {
            enumParser.Parse();
            enumParser.Generate();
        }

        Log.Information("Generated {EnumCount} enums and {TagCount} tags in {Elapsed}ms", enumParser.GeneratedCount, enumParser.GeneratedTagCount, enumWatch.ElapsedMilliseconds);

        var bitflagWatch = Stopwatch.StartNew();
        var bitflagParser = new BitflagParser(xml);
        using (UsingStopwatch(bitflagWatch)) {
            bitflagParser.Parse();
            bitflagParser.Generate();
        }

        Log.Information("Generated {BitflagCount} bitflags and {FlagCount} flags in {Elapsed}ms", bitflagParser.GeneratedCount, bitflagParser.GeneratedFlagCount,
            bitflagWatch.ElapsedMilliseconds);

        var bitfieldWatch = Stopwatch.StartNew();
        var bitfieldParser = new BitfieldParser(xml);
        using (UsingStopwatch(bitfieldWatch)) {
            bitfieldParser.Parse();
            bitfieldParser.Generate();
        }

        Log.Information("Generated {BitfieldCount} bitfields and {FieldCount} fields in {Elapsed}ms", bitfieldParser.GeneratedCount, bitfieldParser.GeneratedFieldCount,
            bitfieldWatch.ElapsedMilliseconds);

        var niobjectWatch = Stopwatch.StartNew();
        var niobjectParser = new NiObjectParser(xml);
        using (UsingStopwatch(niobjectWatch)) {
            niobjectParser.Parse();
            niobjectParser.Generate();
        }

        Log.Information("Generated {NiObjectCount} niobjects and {FieldCount} fields in {Elapsed}ms", niobjectParser.GeneratedCount, niobjectParser.GeneratedFieldsCount,
            niobjectWatch.ElapsedMilliseconds);

        var structWatch = Stopwatch.StartNew();
        var structParser = new StructParser(xml);
        using (UsingStopwatch(structWatch)) {
            structParser.Parse();
            structParser.Generate();
        }

        Log.Information("Generated {StructCount} structs and {FieldCount} fields in {Elapsed}ms", structParser.GeneratedCount, structParser.GeneratedFieldsCount,
            structWatch.ElapsedMilliseconds);
    }

    private static void RegisterInjections() {
        var injectionWatch = Stopwatch.StartNew();
        using (UsingStopwatch(injectionWatch)) {
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
        }

        Log.Information("Registered {InjectionCount} injectors in {Elapsed}ms", InjectionRegistry.Count, injectionWatch.ElapsedMilliseconds);
    }

    private static void RewriteFiles() {
        var trees = new List<SyntaxTree>();
        var fileMap = new Dictionary<SyntaxTree, string>();

        foreach (var file in Directory.GetFiles(GeneratedOutputPath)) {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
            trees.Add(tree);
            fileMap.Add(tree, file);
        }

        // mscorlib stuff and some common system libs
        MetadataReference[] references = [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
        ];

        CSharpCompilation compilation;

        var compilationWatch = Stopwatch.StartNew();
        using (UsingStopwatch(compilationWatch)) {
            compilation = CSharpCompilation.Create("GeneratedNIFObjects")
                .AddSyntaxTrees(trees)
                .AddReferences(references)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        Log.Information("Created compilation with {TreeCount} syntax trees in {Elapsed}ms", trees.Count, compilationWatch.ElapsedMilliseconds);

        var rewriteWatch = Stopwatch.StartNew();
        using (UsingStopwatch(rewriteWatch)) {
            foreach (var tree in trees) {
                var model = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();

                var rewriter = new Rewriter(model);
                var newRoot = rewriter.Visit(root);

                File.WriteAllText(fileMap[tree], newRoot.NormalizeWhitespace().ToFullString());
            }
        }

        Log.Information("Read methods created for all objects in {Elapsed}ms", rewriteWatch.ElapsedMilliseconds);
    }

    private static StopwatchDisposable UsingStopwatch(Stopwatch watch) {
        return new StopwatchDisposable(watch);
    }

    private class StopwatchDisposable : IDisposable {
        private readonly Stopwatch _stopwatch;

        public StopwatchDisposable(Stopwatch stopwatch) {
            _stopwatch = stopwatch;
        }

        public void Dispose() {
            _stopwatch.Stop();
        }
    }
}