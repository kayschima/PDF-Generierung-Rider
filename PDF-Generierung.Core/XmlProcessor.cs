using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PDF_Generierung.Core;

public class XmlProcessor
{
    private const string DefaultNamespace = "https://portalverbund.d-nrw.de/efa/XSozial-basis/Version_2_4_0";

    public List<(string NodeName, List<string> Values)> GetValuesFromNodes(string xmlPath, List<string> nodeNames,
        string targetNamespace = DefaultNamespace)
    {
        if (string.IsNullOrWhiteSpace(xmlPath))
            throw new ArgumentException("Der Pfad zur XML-Datei darf nicht leer sein.", nameof(xmlPath));

        if (nodeNames == null || nodeNames.Count == 0)
            throw new ArgumentException("Es muss mindestens ein Knotenname angegeben werden.", nameof(nodeNames));

        var doc = XDocument.Load(xmlPath);
        var allNodesValues = new List<(string NodeName, List<string> Values)>();

        foreach (var nodeName in nodeNames)
        {
            var targetNodes = doc.Descendants().Where(e => e.Name.LocalName == nodeName).ToList();

            foreach (var targetNode in targetNodes)
            {
                var values = new List<string>();
                ExtractValues(targetNode, values, targetNode.Name.LocalName);

                // Sortierung anwenden, falls eine Konfigurationsdatei existiert
                var sortedValues = SortValues(nodeName, values);
                allNodesValues.Add((nodeName, sortedValues));
            }
        }

        if (allNodesValues.Count == 0)
        {
            var nodesString = string.Join(", ", nodeNames.Select(n => $"<{n}>"));
            throw new InvalidOperationException(
                $"Keiner der angegebenen Knoten ({nodesString}) wurde in der Datei gefunden.");
        }

        return allNodesValues;
    }

    private List<string> SortValues(string nodeName, List<string> values)
    {
        var sortConfigPath = Path.Combine("config", $"sortOrder_{nodeName}.json");
        if (!File.Exists(sortConfigPath))
        {
            return values;
        }

        try
        {
            var jsonContent = File.ReadAllText(sortConfigPath);
            var sortOrder = JsonSerializer.Deserialize<List<string>>(jsonContent);

            if (sortOrder == null || sortOrder.Count == 0)
            {
                return values;
            }

            // Wir filtern die Werte so, dass nur die in der sortOrder Liste enthaltenen Pfade zurückgegeben werden.
            return values.Where(v =>
            {
                var path = v.Split(':').FirstOrDefault() ?? v;
                var pathWithoutIndex = Regex.Replace(path, @"\[\d+\]", "");
                return sortOrder.Contains(path) || sortOrder.Contains(pathWithoutIndex);
            }).OrderBy(v =>
            {
                var path = v.Split(':').FirstOrDefault() ?? v;
                var index = sortOrder.IndexOf(path);
                if (index == -1)
                {
                    var pathWithoutIndex = Regex.Replace(path, @"\[\d+\]", "");
                    index = sortOrder.IndexOf(pathWithoutIndex);
                }

                return index;
            }).ToList();
        }
        catch
        {
            // Bei Fehlern (z.B. ungültiges JSON) geben wir die unsortierten Werte zurück
            return values;
        }
    }

    public List<(string NodeName, List<string> Values)> FilterPersonNodesBySchule(
        List<(string NodeName, List<string> Values)> nodes, string schule)
    {
        return nodes.Where(node =>
        {
            if (node.NodeName != "person") return true;

            // Suche nach dem 'einrichtung'-Wert innerhalb der person-Werte
            // Das Format ist "person-...-einrichtung: wert"
            var einrichtungValue = node.Values
                .FirstOrDefault(v => v.Contains("-einrichtung:"))
                ?.Split(':').LastOrDefault()?.Trim();

            if (einrichtungValue != null && einrichtungValue.Length >= 4)
            {
                return einrichtungValue.Substring(0, 4) == schule;
            }

            return false;
        }).ToList();
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