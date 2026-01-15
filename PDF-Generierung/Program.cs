namespace PDF_Generierung;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Verwendung: PDF-Generierung.exe <xml_pfad> <pdf_dateiname>");
            Console.WriteLine("Beispiel: PDF-Generierung.exe daten.xml ergebnis.pdf");
            return;
        }

        var xmlPath = args[0];
        var outputPdf = args[1];

        if (!File.Exists(xmlPath))
        {
            Console.WriteLine($"Fehler: Die XML-Datei '{xmlPath}' wurde nicht gefunden.");
            return;
        }

        try
        {
            Console.WriteLine($"Verarbeite Datei: {Path.GetFileName(xmlPath)}...");

            var xmlProcessor = new XmlProcessor();
            var values = xmlProcessor.GetVorgangValues(xmlPath);

            var pdfGenerator = new PdfGenerator();
            pdfGenerator.GeneratePdf(values, outputPdf);

            Console.WriteLine("Verarbeitung erfolgreich abgeschlossen.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Details: {ex.InnerException.Message}");
        }
    }
}