using System.Text;
using ts89070.Domain;

namespace ts89070.Data.CSV;

public sealed class PersonCsvStore
{
    public string CsvPath { get; }
    
    private readonly object _fsGate = new();

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
        if (p == null) throw new ArgumentNullException(nameof(p));
        var line = ToCsv(p);

        lock (_fsGate)
        {
            using var fs = new FileStream(CsvPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            fs.Seek(0, SeekOrigin.End);

            bool needNewLine = fs.Length > 0;
            using var sw = new StreamWriter(fs, new UTF8Encoding(false), leaveOpen: true) { NewLine = "\n" };

            if (needNewLine) sw.WriteLine();
            sw.WriteLine(line);
        }
    }

    private static string ToCsv(Person p)
    {
        var last = (p.Lastname ?? string.Empty).Trim();
        var first = (p.Name ?? string.Empty).Trim();
        var zip = (p.Zipcode ?? string.Empty).Trim();
        var city = (p.City ?? string.Empty).Trim();
        var zipCity = string.IsNullOrEmpty(zip) ? city : $"{zip} {city}";
        var colorId = ColorMapper.ResolveColorId(p.Color);

        return string.Join(", ", new[] 
        { 
            QuoteCsv(last), 
            QuoteCsv(first), 
            QuoteCsv(zipCity), 
            QuoteCsv(colorId.ToString()) 
        });
    }
    
    private static string QuoteCsv(string? value)
    {
        value ??= string.Empty;
        bool needQuote = value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
        if (!needQuote) return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}