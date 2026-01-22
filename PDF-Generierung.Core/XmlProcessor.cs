using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PDF_Generierung.Core;

public class XmlProcessor
{
    private const string DefaultNamespace = "https://portalverbund.d-nrw.de/efa/XSozial-basis/Version_2_4_0";

    public List<(string NodeName, string TableTitle, List<string> Values)> GetValuesFromNodes(string xmlPath,
        List<string> nodeNames,
        string targetNamespace = DefaultNamespace)
    {
        if (string.IsNullOrWhiteSpace(xmlPath))
            throw new ArgumentException("Der Pfad zur XML-Datei darf nicht leer sein.", nameof(xmlPath));

        if (nodeNames == null || nodeNames.Count == 0)
            throw new ArgumentException("Es muss mindestens ein Knotenname angegeben werden.", nameof(nodeNames));

        var doc = XDocument.Load(xmlPath);
        var allNodesValues = new List<(string NodeName, string TableTitle, List<string> Values)>();

        foreach (var nodeName in nodeNames)
        {
            var targetNodes = doc.Descendants().Where(e => e.Name.LocalName == nodeName).ToList();

            foreach (var targetNode in targetNodes)
            {
                var values = new List<string>();
                ExtractValues(targetNode, values, targetNode.Name.LocalName);

                // Sortierung anwenden, falls eine Konfigurationsdatei existiert
                var (sortedValues, tableTitle) = SortValues(nodeName, values);
                allNodesValues.Add((nodeName, tableTitle, sortedValues));
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

    private (List<string> Values, string TableTitle) SortValues(string nodeName, List<string> values)
    {
        var sortConfigPath = Path.Combine("config", $"sortOrder_{nodeName}.json");
        var defaultTitle = $"<{nodeName}>";

        if (!File.Exists(sortConfigPath))
        {
            return (values, defaultTitle);
        }

        try
        {
            var jsonContent = File.ReadAllText(sortConfigPath);
            var config = JsonSerializer.Deserialize<SortConfig>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null || config.Mappings == null || config.Mappings.Count == 0)
            {
                return (values, config?.TableTitle ?? defaultTitle);
            }

            var sortOrder = config.Mappings;
            var tableTitle = config.TableTitle ?? defaultTitle;
            var sortKeys = sortOrder.Keys.ToList();

            // Wir filtern die Werte so, dass nur die in der sortOrder Liste enthaltenen Pfade zurückgegeben werden.
            // Dabei ersetzen wir den technischen Pfad durch das Label aus der Konfiguration.
            var sortedList = values.Select(v =>
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

            return (sortedList, tableTitle);
        }
        catch
        {
            // Bei Fehlern (z.B. ungültiges JSON) geben wir die unsortierten Werte zurück
            return (values, defaultTitle);
        }
    }

    public List<(string NodeName, string TableTitle, List<string> Values)> FilterPersonNodesBySchule(
        List<(string NodeName, string TableTitle, List<string> Values)> nodes, string schule)
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

    private class SortConfig
    {
        public string TableTitle { get; set; }
        public Dictionary<string, string> Mappings { get; set; }
    }
}