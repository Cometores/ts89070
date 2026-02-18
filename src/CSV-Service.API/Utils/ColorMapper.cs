namespace CSV;

public static class ColorMapper
{
    private const int FALLBACK_COLOR_ID = 7;
    
    public static readonly Dictionary<int, string> ColorMap = new()
    {
        { 1, "blau" }, 
        { 2, "grün" }, 
        { 3, "violett" }, 
        { 4, "rot" },
        { 5, "gelb" }, 
        { 6, "türkis" }, 
        { 7, "weiß" }
    };
    
    public static int ResolveColorId(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) 
            return FALLBACK_COLOR_ID;
        
        var key = color.Trim();
        if (ReverseColorMap.TryGetValue(key, out var id)) 
            return id;
        if (int.TryParse(key, out var n) && n is >= 1 and <= FALLBACK_COLOR_ID) 
            return n;
        
        return FALLBACK_COLOR_ID;
    }

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
}