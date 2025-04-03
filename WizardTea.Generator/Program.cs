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
                    Log.Error($"status is not success while fetching xml: {response.StatusCode}");

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
                Log.Error($"http request failed: {ex.Message}");
            } catch (Exception ex) {
                Log.Error($"an error occurred: {ex.Message}");
            }
        }

        private static void GenerateCode(XDocument xml) {
            var enumParser = new EnumParser(xml);
            enumParser.Parse();
            enumParser.Generate();
            Log.Information("generated enums");

            RegisterInjections();

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
                
                Use(SystemVector3ToXYZ).For("ByteVector3"),
                Use(XYZToSystemVector3).For("ByteVector3")
            );

            Log.Information("registered injections");
        }
    }
}