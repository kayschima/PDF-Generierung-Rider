using System.Text.Json;

namespace PDF_Generierung;

public static class FilterSettings
{
    private static readonly HashSet<string> KeysToRemove;

    static FilterSettings()
    {
        try
        {
            var fileName = "filterSettings.json";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (!File.Exists(path)) path = fileName;

            var json = File.ReadAllText(path);
            var keys = JsonSerializer.Deserialize<List<string>>(json);
            KeysToRemove = new HashSet<string>(keys ?? [], StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der filterSettings.json: {ex.Message}");
            KeysToRemove = [];
        }
    }

    public static void CleanseList(List<string> values)
    {
        if (values == null) return;

        values.RemoveAll(v =>
        {
            var parts = v.Split(':', 2);
            if (parts.Length == 0) return false;

            var key = parts[0].Trim();

            // Prüfe, ob eines der Fragmente aus KeysToRemove im aktuellen Key enthalten ist
            return KeysToRemove.Any(filterFragment =>
                key.Contains(filterFragment, StringComparison.OrdinalIgnoreCase));
        });
    }
}