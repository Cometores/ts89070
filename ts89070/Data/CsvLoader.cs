using System.Text;
using System.Text.RegularExpressions;
using ts89070.Domain;

namespace ts89070.Services;

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

        // Склеим «рваные» записи: копим буфер, пока не получим >=4 поля (3 запятые)
        var records = CoalesceRecords(rawLines);

        var persons = new List<Person>(records.Count);
        var id = 0;

        foreach (var rec in records)
        {
            // Ожидаем 4 поля: Lastname, Firstname, "<ZIP> <City>", ColorId
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
                // fallback: всё поле -> city, ZIP пустой
                zipcode = string.Empty;
                city = zipCity;
            }

            if (!int.TryParse(colorRaw, out var colorId) || !ColorMapper.ColorMap.TryGetValue(colorId, out var colorName))
                continue; // некорректный ColorId — пропускаем

            persons.Add(new Person
            {
                Id = ++id,                // последовательная нумерация
                Name = first,
                Lastname = last,
                Zipcode = zipcode,
                City = city,
                Color = colorName
            });
        }

        return persons;
    }

    /// <summary>
    /// Наращивает строки, пока в буфере не окажется как минимум 4 поля (3 запятые).
    /// Это чинит случаи вида:
    /// "Bart, Bertram, "   \n
    /// "12313 Wasweißich, 1"
    /// </summary>
    private static List<string> CoalesceRecords(string[] lines)
    {
        var result = new List<string>();
        var sb = new StringBuilder();

        foreach (var raw in lines)
        {
            var line = (raw ?? string.Empty).TrimEnd();

            if (sb.Length > 0)
                sb.Append(' ');           // добавляем пробел между склеенными кусками

            sb.Append(line);

            // Смотрим, достаточно ли полей: 4 поля => минимум 3 запятые
            if (CountCommas(sb) >= 3)
            {
                var candidate = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(candidate))
                    result.Add(candidate);

                sb.Clear();
            }
        }

        // Если файл закончился, а буфер остался — попробуем добавить как есть
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
