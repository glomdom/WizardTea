using System.Xml.Linq;
using WizardTea.Generator.Parsers;

namespace WizardTea.Generator {
    internal static class Program {
        private const string XmlUrl = "https://raw.githubusercontent.com/niftools/nifxml/refs/heads/develop/nif.xml";
        private const string XmlCachePath = "obj/WizardTeaData/cached.xml";
        private const string EtagCachePath = "obj/WizardTeaData/etag.txt";
        private const string GeneratedOutputPath = "Generated/";

        public static async Task Main() {
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
                    Console.WriteLine("no xml updates (etags matched)");
                    if (!File.Exists(XmlCachePath)) return;

                    var cachedXml = await File.ReadAllTextAsync(XmlCachePath);
                    var cachedDoc = XDocument.Parse(cachedXml);
                    GenerateCode(cachedDoc);

                    return;
                }

                if (!response.IsSuccessStatusCode) {
                    Console.WriteLine($"error while fetching xml: {response.StatusCode}");
                    return;
                }

                Console.WriteLine("downloading new xml");

                var newXml = await response.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(XmlCachePath, newXml);

                if (response.Headers.ETag != null) {
                    await File.WriteAllTextAsync(EtagCachePath, response.Headers.ETag.Tag);
                }

                var doc = XDocument.Parse(newXml);
                GenerateCode(doc);
            } catch (HttpRequestException ex) {
                Console.WriteLine($"http request failed: {ex.Message}");
            } catch (Exception ex) {
                Console.WriteLine($"an error occurred: {ex.Message}");
            }
        }

        private static void GenerateCode(XDocument xml) {
            var enumParser = new EnumParser(xml);
            enumParser.Parse();
            enumParser.Generate();
            Console.WriteLine("generated enums");

            var structParser = new StructParser(xml);
            structParser.Parse();
            structParser.Generate();
            Console.WriteLine("generated structs");
        }
    }
}