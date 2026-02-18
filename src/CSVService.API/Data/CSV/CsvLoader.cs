using System.Text;
using System.Text.RegularExpressions;
using CSVService.API.Domain;
using CSVService.API.Utils;

namespace CSVService.API.Data.CSV;

public sealed class CsvLoader
{

    // ZIP + City: "12345 Berlin" -> ("12345","Berlin")
    private static readonly Regex ZipCityRegex = new(@"^\s*(\d{4,5})\s+(.*)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<Person> Load(string csvPath)
    {
        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"CSV not found: {csvPath}");

        var rawLines = File.ReadAllLines(csvPath, Encoding.UTF8);

        // glue together the “broken” records: accumulate the buffer until we get >=4 fields (3 commas)
        var records = CoalesceRecords(rawLines);

        var persons = new List<Person>(records.Count);
        var id = 0;

        foreach (var rec in records)
        {
            // Expecting 4 fields: Lastname, Firstname, “<ZIP> <City>”, ColorId
            var parts = SplitByComma(rec);
            if (parts.Length < 4) continue;

            var last = parts[0].Trim();
            var first = parts[1].Trim();
            var zipCity = parts[2].Trim();
            var colorRaw = parts[3].Trim();

            if (string.IsNullOrWhiteSpace(last) || string.IsNullOrWhiteSpace(first) ||
                string.IsNullOrWhiteSpace(zipCity) || string.IsNullOrWhiteSpace(colorRaw))
                continue;

            string zipcode;
            string city;

            var m = ZipCityRegex.Match(zipCity);
            if (m.Success)
            {
                zipcode = m.Groups[1].Value;
                city = m.Groups[2].Value.Trim();
            }
            else
            {
                // fallback: entire field -> city, ZIP empty
                zipcode = string.Empty;
                city = zipCity;
            }

            if (!int.TryParse(colorRaw, out var colorId) || !ColorMapper.ColorMap.TryGetValue(colorId, out var colorName))
                continue; // incorrect ColorId — skip

            persons.Add(new Person
            {
                Id = ++id,
                Name = first,
                Lastname = last,
                Zipcode = zipcode,
                City = city,
                Color = colorName,
            });
        }

        return persons;
    }


    private static List<string> CoalesceRecords(string[] lines)
    {
        var result = new List<string>();
        var sb = new StringBuilder();

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();

            if (sb.Length > 0)
                sb.Append(' ');

            sb.Append(line);

            if (CountCommas(sb) >= 3)
            {
                var candidate = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(candidate))
                    result.Add(candidate);

                sb.Clear();
            }
        }

        // If the file has ended but the buffer remains, let's try adding it as is.
        if (sb.Length > 0)
        {
            var leftover = sb.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(leftover))
                result.Add(leftover);
        }

        return result;
    }

    private static int CountCommas(StringBuilder sb)
    {
        var cnt = 0;
        for (int i = 0; i < sb.Length; i++)
            if (sb[i] == ',') cnt++;
        return cnt;
    }

    private static string[] SplitByComma(string input)
    {
        var parts = new List<string>(4);
        int taken = 0;
        int last = 0;

        for (int i = 0; i < input.Length && taken < 3; i++)
        {
            if (input[i] == ',')
            {
                parts.Add(input.Substring(last, i - last));
                last = i + 1;
                taken++;
            }
        }

        parts.Add(last <= input.Length ? input[last..] : string.Empty);
        return parts.ToArray();
    }
}
