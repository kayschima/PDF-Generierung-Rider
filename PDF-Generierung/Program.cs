namespace PDF_Generierung;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine(
                "Verwendung: PDF-Generierung.exe <pdf_titel> <xml_pfad> <pdf_dateiname> <xml_knotenname1> [<xml_knotenname2> ...]");
            Console.WriteLine("Beispiel: PDF-Generierung.exe \"Mein Bericht\" daten.xml ergebnis.pdf vorgang");
            Console.WriteLine("Beispiel: PDF-Generierung.exe \"Antragsdaten\" daten.xml ergebnis.pdf person dritte");
            return;
        }

        var pdfTitle = args[0];
        var xmlPath = args[1];
        var outputPdf = args[2];
        var targetNodes = args.Skip(3).ToList();

        if (!File.Exists(xmlPath))
        {
            Console.WriteLine($"Fehler: Die XML-Datei '{xmlPath}' wurde nicht gefunden.");
            return;
        }

        try
        {
            Console.WriteLine($"Verarbeite Datei: {Path.GetFileName(xmlPath)}...");
            Console.WriteLine($"Suche nach Knoten: {string.Join(", ", targetNodes.Select(n => $"<{n}>"))}");

            var xmlProcessor = new XmlProcessor();
            var allNodesValues = xmlProcessor.GetValuesFromNodes(xmlPath, targetNodes);

            var pdfGenerator = new PdfGenerator();
            pdfGenerator.GeneratePdf(allNodesValues, outputPdf, pdfTitle);

            Console.WriteLine("Verarbeitung erfolgreich abgeschlossen.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Details: {ex.InnerException.Message}");
        }
    }
}