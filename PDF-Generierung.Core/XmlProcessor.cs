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
            var sortOrder = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

            if (sortOrder == null || sortOrder.Count == 0)
            {
                return values;
            }

            var sortKeys = sortOrder.Keys.ToList();

            // Wir filtern die Werte so, dass nur die in der sortOrder Liste enthaltenen Pfade zurückgegeben werden.
            // Dabei ersetzen wir den technischen Pfad durch das Label aus der Konfiguration.
            return values.Select(v =>
                {
                    var parts = v.Split(':', 2);
                    var path = parts[0].Trim();
                    var value = parts.Length > 1 ? parts[1].Trim() : "";

                    var pathWithoutIndex = Regex.Replace(path, @"\[\d+\]", "");

                    if (sortOrder.TryGetValue(path, out var label) ||
                        sortOrder.TryGetValue(pathWithoutIndex, out label))
                    {
                        return $"{label}: {value}";
                    }

                    return null;
                })
                .Where(v => v != null)
                .OrderBy(v =>
                {
                    var label = v.Split(':', 2)[0].Trim();
                    // Wir suchen den Index basierend auf dem Label in der sortOrder
                    var key = sortOrder.FirstOrDefault(x => x.Value == label).Key;
                    return sortKeys.IndexOf(key);
                })
                .ToList()!;
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
            // Da die Werte bereits sortiert und gelabelt sind, suchen wir nach dem Label "Schule"
            var einrichtungValue = node.Values
                .FirstOrDefault(v => v.StartsWith("Schule:"))
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