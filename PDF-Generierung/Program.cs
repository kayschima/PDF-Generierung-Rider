namespace PDF_Generierung;

class Program
{
    static void Main(string[] args)
    {
        // Pfad zur XML-Datei bestimmen (Standard oder via Argument)
        string defaultXmlName = "urn-de-xta-messageid-dataport_xta_210-fc5cde98-f19f-48e9-8b6a-c74d752646bd.xml";
        string xmlPath = args.Length > 0 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", defaultXmlName);
        
        if (!File.Exists(xmlPath))
        {
            xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultXmlName);
        }

        if (!File.Exists(xmlPath))
        {
            Console.WriteLine($"Warnung: XML-Datei '{xmlPath}' wurde nicht gefunden.");
            Console.WriteLine("Bitte geben Sie den Pfad als Argument an: PDF-Generierung.exe <pfad_zu_xml>");
            return;
        }

        try
        {
            Console.WriteLine($"Verarbeite Datei: {Path.GetFileName(xmlPath)}...");
            
            XmlProcessor xmlProcessor = new XmlProcessor();
            List<string> values = xmlProcessor.GetVorgangValues(xmlPath);

            PdfGenerator pdfGenerator = new PdfGenerator();
            string outputPdf = "Vorgang_Werte.pdf";
            pdfGenerator.GeneratePdf(values, outputPdf);
            
            Console.WriteLine("Verarbeitung erfolgreich abgeschlossen.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Details: {ex.InnerException.Message}");
            }
        }
    }
}