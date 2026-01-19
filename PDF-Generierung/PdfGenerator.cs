using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace PDF_Generierung;

public class PdfGenerator
{
    private const double Margin = 40;
    private const double RowHeight = 20;

    private XGraphics? _gfx;
    private PdfPage? _page;
    private double _yPoint;

    public void GeneratePdf(List<(string NodeName, List<string> Values)> allNodesValues, string filename,
        string pdfTitle)
    {
        using var document = new PdfDocument();
        document.Info.Title = pdfTitle;

        _page = document.AddPage();
        _gfx = XGraphics.FromPdfPage(_page);

        DrawMainTitle(pdfTitle);

        _yPoint = Margin + 60;

        var countsByNode = allNodesValues.GroupBy(n => n.NodeName)
            .ToDictionary(g => g.Key, g => 0);

        foreach (var (nodeName, values) in allNodesValues)
        {
            countsByNode[nodeName]++;
            var currentInstance = countsByNode[nodeName];
            var totalInstancesForThisNode = allNodesValues.Count(n => n.NodeName == nodeName);

            FilterSettings.CleanseList(values);

            var tableData = values.Select(ProcessValueLine).ToList();

            var title = totalInstancesForThisNode > 1
                ? $"<{nodeName}> (Instanz {currentInstance})"
                : $"<{nodeName}>";

            DrawTable(title, tableData);
        }

        _gfx?.Dispose();
        AddPageNumbers(document);

        document.Save(filename);
        Console.WriteLine($"PDF erfolgreich erstellt: {filename}");
    }

    private void DrawMainTitle(string title)
    {
        if (_gfx == null || _page == null) return;
        var titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
        _gfx.DrawString(title, titleFont, XBrushes.Black,
            new XRect(Margin, Margin, _page.Width.Point - 2 * Margin, 40), XStringFormats.Center);
    }

    private KeyValuePair<string, string> ProcessValueLine(string line)
    {
        var parts = line.Split(':', 2);
        var technicalKey = parts[0].Trim();
        var rawValue = parts.Length > 1 ? parts[1].Trim() : "";

        var transformedValue = TransformValue(rawValue);
        var displayKey = LabelMapper.GetFriendlyName(technicalKey);

        return new KeyValuePair<string, string>(displayKey, transformedValue);
    }

    private string TransformValue(string value)
    {
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
            return "ja";
        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
            return "nein";

        if (DateTime.TryParse(value, out var dateTime) && value.Contains("-"))
        {
            if (value.Contains("T"))
                return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
            return dateTime.ToString("dd.MM.yyyy");
        }

        return value;
    }

    private void AddPageNumbers(PdfDocument document)
    {
        var footerFont = new XFont("Arial", 10, XFontStyleEx.Regular);
        for (var i = 0; i < document.PageCount; i++)
        {
            var page = document.Pages[i];
            using var footerGfx = XGraphics.FromPdfPage(page);
            var pageNumberText = $"Seite {i + 1} von {document.PageCount}";
            footerGfx.DrawString(pageNumberText, footerFont, XBrushes.Black,
                new XRect(0, page.Height.Point - Margin + 10, page.Width.Point, 20), XStringFormats.Center);
        }
    }

    /// <summary>
    ///     Entfernt alle Einträge aus der Liste, die mit dem angegebenen Key beginnen.
    /// </summary>
    public void RemoveKey(List<string> values, string keyToRemove)
    {
        if (values == null || string.IsNullOrWhiteSpace(keyToRemove)) return;

        // Wir suchen nach dem Key gefolgt von einem Doppelpunkt
        var searchPattern = keyToRemove.Trim() + ":";

        // RemoveAll entfernt effizient alle passenden Einträge aus der Liste
        values.RemoveAll(v => v.StartsWith(searchPattern, StringComparison.OrdinalIgnoreCase));
    }

    public void DrawTable(string title, List<KeyValuePair<string, string>> items)
    {
        if (_gfx == null || _page == null) return;

        var headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
        var cellFont = new XFont("Arial", 9, XFontStyleEx.Regular);
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
        if (_page == null) return;

        if (_yPoint + neededHeight > _page.Height.Point - Margin)
        {
            _gfx?.Dispose(); // Vorheriges XGraphics-Objekt freigeben
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
            return new FontResolverInfo("Arial");
        }

        public byte[]? GetFont(string faceName)
        {
            return null;
        }
    }
}