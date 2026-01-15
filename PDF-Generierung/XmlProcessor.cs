using System.Xml.Linq;

namespace PDF_Generierung;

public class XmlProcessor
{
    private const string DefaultNamespace = "https://portalverbund.d-nrw.de/efa/XSozial-basis/Version_2_4_0";

    public List<string> GetValuesFromNode(string xmlPath, string nodeName, string targetNamespace = DefaultNamespace)
    {
        if (string.IsNullOrWhiteSpace(xmlPath))
            throw new ArgumentException("Der Pfad zur XML-Datei darf nicht leer sein.", nameof(xmlPath));

        var doc = XDocument.Load(xmlPath);
        XNamespace ns = targetNamespace;

        // Suche nach dem Knoten im angegebenen Namespace oder ohne Namespace
        var targetNode = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == nodeName);

        if (targetNode == null)
            throw new InvalidOperationException($"Knoten <{nodeName}> wurde in der Datei nicht gefunden.");

        var values = new List<string>();
        ExtractValues(targetNode, values, targetNode.Name.LocalName);
        return values;
    }

    private void ExtractValues(XElement element, List<string> values, string currentPath)
    {
        var childGroups = element.Elements().GroupBy(e => e.Name);

        foreach (var group in childGroups)
        {
            var index = 1;
            var isMultiple = group.Count() > 1;

            foreach (var child in group)
            {
                var suffix = isMultiple ? $"[{index}]" : "";
                var childPath = $"{currentPath}-{child.Name.LocalName}{suffix}";

                if (!child.HasElements && !string.IsNullOrWhiteSpace(child.Value))
                    values.Add($"{childPath}: {child.Value.Trim()}");

                ExtractValues(child, values, childPath);
                index++;
            }
        }
    }
}