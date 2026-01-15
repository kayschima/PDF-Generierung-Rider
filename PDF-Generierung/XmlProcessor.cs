using System.Xml.Linq;
using System.Linq;

namespace PDF_Generierung;

public class XmlProcessor
{
    public List<string> GetVorgangValues(string xmlPath)
    {
        if (!File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"XML-Datei nicht gefunden: {xmlPath}");
        }

        XDocument doc = XDocument.Load(xmlPath);
        XNamespace xsb = "https://portalverbund.d-nrw.de/efa/XSozial-basis/Version_2_4_0";
        
        var vorgang = doc.Descendants(xsb + "vorgang").FirstOrDefault();

        if (vorgang == null)
        {
            throw new InvalidOperationException("Node <xsb:vorgang> nicht gefunden.");
        }

        List<string> values = new List<string>();
        ExtractValues(vorgang, values, vorgang.Name.LocalName);
        return values;
    }

    private void ExtractValues(XElement element, List<string> values, string currentPath)
    {
        var childGroups = element.Elements().GroupBy(e => e.Name);

        foreach (var group in childGroups)
        {
            int index = 1;
            bool isMultiple = group.Count() > 1;

            foreach (var child in group)
            {
                string suffix = isMultiple ? $"[{index}]" : "";
                string childPath = $"{currentPath}/{child.Name.LocalName}{suffix}";

                if (!child.HasElements && !string.IsNullOrWhiteSpace(child.Value))
                {
                    values.Add($"{childPath}: {child.Value.Trim()}");
                }

                ExtractValues(child, values, childPath);
                index++;
            }
        }
    }
}
