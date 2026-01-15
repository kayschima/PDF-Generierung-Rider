namespace PDF_Generierung;

class Program
{
    static void Main(string[] args)
    {
        string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "urn-de-xta-messageid-dataport_xta_210-fc5cde98-f19f-48e9-8b6a-c74d752646bd.xml");
        
        if (!File.Exists(xmlPath))
        {
            xmlPath = "urn-de-xta-messageid-dataport_xta_210-fc5cde98-f19f-48e9-8b6a-c74d752646bd.xml";
        }

        try
        {
            XmlProcessor xmlProcessor = new XmlProcessor();
            List<string> values = xmlProcessor.GetVorgangValues(xmlPath);

            PdfGenerator pdfGenerator = new PdfGenerator();
            pdfGenerator.GeneratePdf(values, "Vorgang_Werte.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
    }
}