namespace PDF_Generierung;

public static class FilterSettings
{
    // Hier alle Keys eintragen, die NICHT im PDF erscheinen sollen
    private static readonly HashSet<string> KeysToRemove = new(StringComparer.OrdinalIgnoreCase)
    {
        "dritte-dritteTyp-code",
        "dritte-postfach-postfachID",
        "dritte-postfach-postfachTyp-code",
        "dritte-postfach-provider",
        "dritte-drittePerson-nichtNatuerlichePerson-kommunikation[1]-kanal-code",
        "dritte-drittePerson-nichtNatuerlichePerson-kommunikation[2]-kanal-code",
        "dritte-drittePerson-nichtNatuerlichePerson-kommunikation[3]-kanal-code"
        // Einfach weitere Keys hier hinzufügen
    };

    public static void CleanseList(List<string> values)
    {
        if (values == null) return;

        values.RemoveAll(v =>
        {
            var parts = v.Split(':', 2);
            if (parts.Length == 0) return false;

            var key = parts[0].Trim();
            return KeysToRemove.Contains(key);
        });
    }
}