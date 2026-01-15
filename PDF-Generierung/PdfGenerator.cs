using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace PDF_Generierung;

public class PdfGenerator
{
    private const double Margin = 40;
    private const double RowHeight = 20;
    private XGraphics _gfx;
    private PdfPage _page;
    private double _yPoint;

    public void GeneratePdf(List<string> values, string filename, string targetNode)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"Werte aus {targetNode}";

        _page = document.AddPage();
        _gfx = XGraphics.FromPdfPage(_page);
        _yPoint = Margin;

        // Beispielaufruf der neuen Tabellenfunktion
        var tableData = values.Select(v =>
        {
            var parts = v.Split(':', 2);
            return new KeyValuePair<string, string>(parts[0], parts.Length > 1 ? parts[1].Trim() : "");
        }).ToList();

        DrawTable($"Extrahiert aus <{targetNode}>", tableData);

        document.Save(filename);
        Console.WriteLine($"PDF erfolgreich erstellt: {filename}");
    }

    public void DrawTable(string title, List<KeyValuePair<string, string>> items)
    {
        var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
        var cellFont = new XFont("Arial", 10, XFontStyleEx.Regular);
        var tableWidth = _page.Width.Point - 2 * Margin;
        var colWidth = tableWidth / 2;

        // Prüfung auf Seitenende für den Header
        CheckPageFlow(RowHeight);

        // 1. Header zeichnen (Roter Hintergrund, weiße Schrift)
        _gfx.DrawRectangle(XBrushes.Red, Margin, _yPoint, tableWidth, RowHeight);
        _gfx.DrawString(title, headerFont, XBrushes.White,
            new XRect(Margin + 5, _yPoint, tableWidth, RowHeight), XStringFormats.CenterLeft);

        _yPoint += RowHeight;

        // 2. Datenzeilen zeichnen
        foreach (var item in items)
        {
            CheckPageFlow(RowHeight);

            // Rahmen für Zellen
            _gfx.DrawRectangle(XPens.Black, Margin, _yPoint, colWidth, RowHeight);
            _gfx.DrawRectangle(XPens.Black, Margin + colWidth, _yPoint, colWidth, RowHeight);

            // Texte in die Spalten schreiben
            _gfx.DrawString(item.Key, cellFont, XBrushes.Black,
                new XRect(Margin + 5, _yPoint, colWidth - 10, RowHeight), XStringFormats.CenterLeft);
            _gfx.DrawString(item.Value, cellFont, XBrushes.Black,
                new XRect(Margin + colWidth + 5, _yPoint, colWidth - 10, RowHeight), XStringFormats.CenterLeft);

            _yPoint += RowHeight;
        }

        _yPoint += 10; // Kleiner Abstand nach der Tabelle
    }

    private void CheckPageFlow(double neededHeight)
    {
        if (_yPoint + neededHeight > _page.Height.Point - Margin)
        {
            _page = _page.Owner.AddPage();
            _gfx = XGraphics.FromPdfPage(_page);
            _yPoint = Margin;
        }
    }

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
}