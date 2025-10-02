using System.Text;
using ts89070.Domain;

namespace ts89070.Services;

public sealed class PersonCsvStore
{
    private readonly object _fsGate = new();
    public string CsvPath { get; }

    // имя цвета -> числовой код (1..7)
    private static readonly Dictionary<string, int> ReverseColorMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["blau"] = 1,
            ["grün"] = 2, ["gruen"] = 2,
            ["violett"] = 3,
            ["rot"] = 4,
            ["gelb"] = 5,
            ["türkis"] = 6, ["tuerkis"] = 6,
            ["weiß"] = 7, ["weiss"] = 7
        };

    public PersonCsvStore(IConfiguration cfg)
    {
        var location = cfg["DataSources:PersonCsv:Location"];
        if (string.IsNullOrWhiteSpace(location))
            location = AppContext.BaseDirectory;

        var filename = cfg["DataSources:PersonCsv:Filename"] ?? "sample-input.csv";
        CsvPath = Path.Combine(location, filename);
    }

    public void Append(Person p)
    {
        // Формат: "Lastname, Firstname, <ZIP> <City>, <ColorId>"
        var last = Sanitize(p.Lastname);
        var first = Sanitize(p.Name);
        var zip = (p.Zipcode ?? string.Empty).Trim();
        var city = (p.City ?? string.Empty).Trim();
        var zipCity = string.IsNullOrEmpty(zip) ? city : $"{zip} {city}";
        var colorId = ResolveColorId(p.Color);

        var line = $"{last}, {first}, {zipCity}, {colorId}";

        lock (_fsGate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CsvPath)!);

            using var fs = new FileStream(CsvPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            fs.Seek(0, SeekOrigin.End);

            bool needLeadingNewline = false;
            long endPos = fs.Position;
            if (endPos > 0)
            {
                fs.Position = endPos - 1;
                int b1 = fs.ReadByte();
                if (b1 != '\n')
                {
                    if (endPos >= 2)
                    {
                        fs.Position = endPos - 2;
                        int b2 = fs.ReadByte();
                        needLeadingNewline = b1 != '\r' && b2 != '\n';
                    }
                    else
                    {
                        needLeadingNewline = b1 != '\r';
                    }
                }
                fs.Position = endPos;
            }

            using var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                NewLine = "\n"
            };

            if (needLeadingNewline) sw.WriteLine();
            sw.WriteLine(line);
            sw.Flush();
            fs.Flush(true);
        }
    }

    private static string Sanitize(string? s) => (s ?? string.Empty).Replace(",", " ").Trim();

    private static int ResolveColorId(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return 7;
        var key = color.Trim();
        if (ReverseColorMap.TryGetValue(key, out var id)) return id;
        if (int.TryParse(key, out var n) && n is >= 1 and <= 7) return n;
        return 7;
    }
}