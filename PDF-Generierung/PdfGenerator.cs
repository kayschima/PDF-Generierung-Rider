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

    public void GeneratePdf(List<(string NodeName, List<string> Values)> allNodesValues, string filename,
        string pdfTitle)
    {
        using var document = new PdfDocument();
        document.Info.Title = pdfTitle;

        _page = document.AddPage();
        _gfx = XGraphics.FromPdfPage(_page);

        // Hauptüberschrift zeichnen
        var titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
        _gfx.DrawString(pdfTitle, titleFont, XBrushes.Black,
            new XRect(Margin, Margin, _page.Width.Point - 2 * Margin, 40), XStringFormats.Center);

        _yPoint = Margin + 60; // Startpunkt für die Tabellen nach der Überschrift

        // Gruppieren nach NodeName, um Instanzen pro Node zählen zu können
        var countsByNode = allNodesValues.GroupBy(n => n.NodeName)
            .ToDictionary(g => g.Key, g => 0);

        foreach (var (nodeName, values) in allNodesValues)
        {
            countsByNode[nodeName]++;
            var currentInstance = countsByNode[nodeName];
            var totalInstancesForThisNode = allNodesValues.Count(n => n.NodeName == nodeName);

            // Liste bereinigen basierend auf den Filter-Einstellungen
            FilterSettings.CleanseList(values);

            var tableData = values.Select(v =>
            {
                var parts = v.Split(':', 2);
                var technicalKey = parts[0].Trim();
                var value = parts.Length > 1 ? parts[1].Trim() : "";

                // Wert-Transformationen
                if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    value = "ja";
                else if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    value = "nein";
                else if (DateTime.TryParse(value, out var dateTime))
                    // Prüfung, ob es wie ein technisches Datum/Zeit aussieht (enthält Bindestriche)
                    if (value.Contains("-"))
                    {
                        // Wenn 'T' enthalten ist, handelt es sich wahrscheinlich um einen Zeitstempel (ISO 8601)
                        if (value.Contains("T"))
                            value = dateTime.ToString("dd.MM.yyyy HH:mm:ss");
                        else
                            value = dateTime.ToString("dd.MM.yyyy");
                    }

                // Nutze die LabelMapper-Klasse
                var displayKey = LabelMapper.GetFriendlyName(technicalKey);

                return new KeyValuePair<string, string>(displayKey, value);
            }).ToList();

            var title = totalInstancesForThisNode > 1
                ? $"<{nodeName}> (Instanz {currentInstance})"
                : $"<{nodeName}>";

            DrawTable(title, tableData);
        }

        // Sicherstellen, dass das letzte XGraphics-Objekt freigegeben wird
        _gfx.Dispose();

        // Seitenzahlen am Ende hinzufügen
        var footerFont = new XFont("Arial", 10, XFontStyleEx.Regular);
        for (int i = 0; i < document.PageCount; i++)
        {
            var page = document.Pages[i];
            using var footerGfx = XGraphics.FromPdfPage(page);
            var pageNumberText = $"Seite {i + 1} von {document.PageCount}";
            footerGfx.DrawString(pageNumberText, footerFont, XBrushes.Black,
                new XRect(0, page.Height.Point - Margin + 10, page.Width.Point, 20), XStringFormats.Center);
        }

        document.Save(filename);
        Console.WriteLine($"PDF erfolgreich erstellt: {filename}");
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
        if (_yPoint + neededHeight > _page.Height.Point - Margin)
        {
            _gfx.Dispose(); // Vorheriges XGraphics-Objekt freigeben
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