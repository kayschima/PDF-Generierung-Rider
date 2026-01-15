namespace PDF_Generierung;

public static class LabelMapper
{
    private static readonly Dictionary<string, string> DritteMappings = new()
    {
        { "dritte-dritteID", "Rolle" },
        {
            "dritte-drittePerson-nichtNatuerlichePerson-anschrift-anschriftInland-gebaeude-postleitzahl", "Postleitzahl"
        },
        { "dritte-drittePerson-nichtNatuerlichePerson-anschrift-anschriftInland-gebaeude-strasse", "Straße" },
        { "dritte-drittePerson-nichtNatuerlichePerson-anschrift-anschriftInland-gebaeude-wohnort", "Wohnort" },
        {
            "dritte-drittePerson-nichtNatuerlichePerson-anschrift-anschriftInland-gebaeude-teilnummerDerHausnummer",
            "Hausnummer"
        },
        {
            "dritte-drittePerson-nichtNatuerlichePerson-anschrift-anschriftInland-gebaeude-zusatzangaben",
            "Zusatzangaben"
        },
        {
            "dritte-drittePerson-nichtNatuerlichePerson-ansprechpartner-nameNatuerlichePerson-familienname-name",
            "Familenname"
        },
        { "dritte-drittePerson-nichtNatuerlichePerson-ansprechpartner-nameNatuerlichePerson-vorname-name", "Vorname" },
        { "dritte-drittePerson-nichtNatuerlichePerson-ansprechpartner-geburt-datum", "Geburtsdatum" },
        { "dritte-drittePerson-nichtNatuerlichePerson-kommunikation[1]-kennung", "Kontakt (Prio 1):" },
        { "dritte-drittePerson-nichtNatuerlichePerson-kommunikation[2]-kennung", "Kontakt (Prio 2):" },
        { "dritte-schriftgutempfaenger", "Schriftgutempfänger" },
        {
            "dritte-drittePerson-nichtNatuerlichePerson-ansprechpartner-staatsangehoerigkeit-staatsangehoerigkeit-code",
            "Staatsangehörigkeit"
        }

        // Hier kannst du bequem alle weiteren Bezeichnungen pflegen
    };

    public static string GetFriendlyName(string technicalKey)
    {
        if (string.IsNullOrWhiteSpace(technicalKey)) return technicalKey;

        // Versuche den Key zu finden, ansonsten gib den Original-Key zurück
        return DritteMappings.GetValueOrDefault(technicalKey.Trim(), technicalKey);
    }
}