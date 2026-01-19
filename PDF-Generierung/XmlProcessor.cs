using System.Xml.Linq;

namespace PDF_Generierung;

public class XmlProcessor
{
    private const string DefaultNamespace = "https://portalverbund.d-nrw.de/efa/XSozial-basis/Version_2_4_0";

    public List<List<string>> GetValuesFromNode(string xmlPath, string nodeName,
        string targetNamespace = DefaultNamespace)
    {
        if (string.IsNullOrWhiteSpace(xmlPath))
            throw new ArgumentException("Der Pfad zur XML-Datei darf nicht leer sein.", nameof(xmlPath));

        var doc = XDocument.Load(xmlPath);
        XNamespace ns = targetNamespace;

        // Suche nach allen Knoten im angegebenen Namespace oder ohne Namespace
        var targetNodes = doc.Descendants().Where(e => e.Name.LocalName == nodeName).ToList();

        if (targetNodes.Count == 0)
            throw new InvalidOperationException($"Knoten <{nodeName}> wurde in der Datei nicht gefunden.");

        var allNodesValues = new List<List<string>>();

        foreach (var targetNode in targetNodes)
        {
            var values = new List<string>();
            ExtractValues(targetNode, values, targetNode.Name.LocalName);
            allNodesValues.Add(values);
        }

        return allNodesValues;
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