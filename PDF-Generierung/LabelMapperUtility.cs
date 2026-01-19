using System.Text.Json;

namespace PDF_Generierung;

public static class LabelMapper
{
    private static readonly Dictionary<string, string> ValueMappings;

    static LabelMapper()
    {
        try
        {
            var json = File.ReadAllText("labelMappings.json");
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

        // Versuche den Key zu finden, ansonsten gib den Original-Key zurück
        return ValueMappings.GetValueOrDefault(technicalKey.Trim(), technicalKey);
    }
}