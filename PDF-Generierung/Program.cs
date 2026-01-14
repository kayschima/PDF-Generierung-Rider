using System.Xml;
using System.Xml.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;

namespace PDF_Generierung;

class Program
{
    // Simple FontResolver implementation if needed
    public class SimpleFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // This is a minimal implementation. In a real scenario, you'd return the correct font.
            // For now, we hope the system has some default or we use the snippet if available.
            return new FontResolverInfo("Arial");
        }

        public byte[] GetFont(string faceName)
        {
            // This is just a stub. Normally you'd return the font data.
            return null; 
        }
    }

    static void Main(string[] args)
    {
        // On Windows, the default resolver usually works if GDI+ is available.
        // For .NET 8, we might need to set it explicitly if it fails.
        
        string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "urn-de-xta-messageid-dataport_xta_210-fc5cde98-f19f-48e9-8b6a-c74d752646bd.xml");
        
        if (!File.Exists(xmlPath))
        {
            xmlPath = "urn-de-xta-messageid-dataport_xta_210-fc5cde98-f19f-48e9-8b6a-c74d752646bd.xml";
        }

        if (!File.Exists(xmlPath))
        {
            Console.WriteLine($"XML-Datei nicht gefunden: {xmlPath}");
            return;
        }

        XDocument doc = XDocument.Load(xmlPath);
        XNamespace xsb = "https://portalverbund.d-nrw.de/efa/XSozial-basis/Version_2_4_0";
        
        var vorgang = doc.Descendants(xsb + "vorgang").FirstOrDefault();

        if (vorgang == null)
        {
            Console.WriteLine("Node <xsb:vorgang> nicht gefunden.");
            return;
        }

        List<string> values = new List<string>();
        ExtractValues(vorgang, values);

        GeneratePdf(values);
    }

    static void ExtractValues(XElement element, List<string> values)
    {
        foreach (var node in element.Nodes())
        {
            if (node is XElement child)
            {
                if (!child.HasElements && !string.IsNullOrWhiteSpace(child.Value))
                {
                    values.Add($"{child.Name.LocalName}: {child.Value.Trim()}");
                }
                ExtractValues(child, values);
            }
        }
    }

    static void GeneratePdf(List<string> values)
    {
        PdfDocument document = new PdfDocument();
        document.Info.Title = "Werte aus xsb:vorgang";

        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        
        // Use standard fonts if possible, or fallback to something that might work.
        // In PDFsharp 6.x, we might need to handle font resolving.
        // For simplicity, we try to use the built-in fonts if available.
        XFont titleFont = new XFont("Arial", 14, XFontStyleEx.Bold);
        XFont textFont = new XFont("Arial", 10, XFontStyleEx.Regular);

        double yPoint = 40;
        double margin = 40;
        double lineSpacing = 15;

        gfx.DrawString("Werte aus <xsb:vorgang>:", titleFont, XBrushes.Black, new XPoint(margin, yPoint));
        yPoint += 30;

        foreach (var value in values)
        {
            if (yPoint > page.Height.Point - margin)
            {
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                yPoint = margin;
            }

            gfx.DrawString(value, textFont, XBrushes.Black, new XPoint(margin, yPoint));
            yPoint += lineSpacing;
        }

        string filename = "Vorgang_Werte.pdf";
        document.Save(filename);
        Console.WriteLine($"PDF erfolgreich erstellt: {filename}");
    }
}