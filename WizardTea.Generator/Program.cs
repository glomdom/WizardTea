using System.Xml.Linq;
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

                    GenerateCode(cachedDoc);

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
                GenerateCode(doc);
            } catch (HttpRequestException ex) {
                Log.Fatal("http request failed: {error}", ex.Message);
            } catch (Exception ex) {
                Log.Fatal("an error occurred: {error}", ex.Message);
            }
        }

        private static void GenerateCode(XDocument xml) {
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
    }
}