namespace PDF_Generierung;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine(
                "Verwendung: PDF-Generierung.exe <xml_pfad> <pdf_dateiname> <xml_knotenname1> [<xml_knotenname2> ...]");
            Console.WriteLine("Beispiel: PDF-Generierung.exe daten.xml ergebnis.pdf vorgang");
            Console.WriteLine("Beispiel: PDF-Generierung.exe daten.xml ergebnis.pdf person dritte");
            return;
        }

        var xmlPath = args[0];
        var outputPdf = args[1];
        var targetNodes = args.Skip(2).ToList();

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
            pdfGenerator.GeneratePdf(allNodesValues, outputPdf);

            Console.WriteLine("Verarbeitung erfolgreich abgeschlossen.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Details: {ex.InnerException.Message}");
        }
    }
}