using System.Xml;

namespace DepartmentResourcesApp.Models;

public class XmlDataDocument
{
    private string filePath = string.Empty;
    private string rootElementName = string.Empty;
    private readonly XmlDocument internalDoc = new();

    public void Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            throw new FileNotFoundException("Вказаний XML-файл не знайдено.", path);

        internalDoc.Load(path);
        filePath = path;
        rootElementName = internalDoc.DocumentElement?.Name ?? string.Empty;
    }

    public string GetFilePath() => filePath;

    public List<string> GetResourceElementNames()
    {
        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (XmlNode node in GetResourceNodes())
        {
            names.Add(node.Name);
        }
        return names.ToList();
    }

    public List<string> GetAttributeNames()
    {
        var attrNames = new SortedSet<string>(StringComparer.Ordinal);
        foreach (XmlNode node in GetResourceNodes())
        {
            if (node.Attributes == null) continue;
            foreach (XmlAttribute attr in node.Attributes)
            {
                attrNames.Add(attr.Name);
            }
        }
        return attrNames.ToList();
    }

    public List<string> GetAttributeValues(string attributeName)
    {
        var values = new SortedSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(attributeName) || attributeName == "Усі атрибути")
            return values.ToList();

        foreach (XmlNode node in GetResourceNodes())
        {
            var attr = node.Attributes?[attributeName];
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Value))
            {
                values.Add(attr.Value);
            }
        }
        return values.ToList();
    }

    public List<XmlNode> GetResourceNodes()
    {
        var result = new List<XmlNode>();
        if (internalDoc.DocumentElement == null)
            return result;

        var nodes = internalDoc.SelectNodes("/*/*/*");
        if (nodes == null)
            return result;

        foreach (XmlNode node in nodes)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                result.Add(node);
            }
        }

        return result;
    }
}
