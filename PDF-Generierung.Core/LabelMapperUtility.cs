using System.Text.Json;

namespace PDF_Generierung.Core;

public static class LabelMapper
{
    private static readonly Dictionary<string, string> ValueMappings;

    static LabelMapper()
    {
        try
        {
            var fileName = "labelMappings.json";
            // Suche die Datei im aktuellen Verzeichnis oder im Verzeichnis der Assembly
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (!File.Exists(path)) path = fileName;

            var json = File.ReadAllText(path);
            ValueMappings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                            ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der labelMappings.json: {ex.Message}");
            ValueMappings = new Dictionary<string, string>();
        }
    }

    public static string GetFriendlyName(string technicalKey)
    {
        if (string.IsNullOrWhiteSpace(technicalKey)) return technicalKey;

        var trimmedKey = technicalKey.Trim();

        // 1. Versuche zuerst eine exakte Übereinstimmung (höchste Priorität und beste Performance)
        if (ValueMappings.TryGetValue(trimmedKey, out var friendlyName)) return friendlyName;

        // 2. Suche nach Fragmenten: Wenn ein Mapping-Key im technischen Key enthalten ist,
        // wird der technische Key komplett durch den Friendly Name ersetzt.
        foreach (var mapping in ValueMappings)
            if (trimmedKey.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
                return mapping.Value;

        return trimmedKey;
    }
}