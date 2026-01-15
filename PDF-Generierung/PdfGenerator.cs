using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;

namespace PDF_Generierung;

public class PdfGenerator
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

    public void GeneratePdf(List<string> values, string filename)
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

        document.Save(filename);
        Console.WriteLine($"PDF erfolgreich erstellt: {filename}");
    }
}
