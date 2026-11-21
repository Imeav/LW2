using System.Xml;
using DepartmentResourcesApp.Models;
using XmlDataDocument = DepartmentResourcesApp.Models.XmlDataDocument;

namespace DepartmentResourcesApp.Strategies;

public class DomStrategy : XmlAnalysisStrategy
{
    private XmlDocument xmlDocument = new();

    public override string Name => "DOM API (XmlDocument)";

    public override List<ResourceRecord> Analyze(
        XmlDataDocument document,
        SearchParameters parameters,
        string elementName)
    {
        string path = document.GetFilePath();
        if (string.IsNullOrWhiteSpace(path))
            return new List<ResourceRecord>();

        xmlDocument = new XmlDocument();
        xmlDocument.Load(path);

        string query = parameters.BuildXPathQuery(elementName);
        var matchedNodes = FindMatchingNodes(query);
        var results = new List<ResourceRecord>();

        foreach (XmlNode node in matchedNodes)
        {
            var record = MapNodeToRecord(node);
            if (record.Matches(parameters))
            {
                results.Add(record);
            }
        }

        return results;
    }

    public List<XmlNode> FindMatchingNodes(string query)
    {
        var list = new List<XmlNode>();
        var nodeList = xmlDocument.SelectNodes(query);
        if (nodeList == null)
            return list;

        foreach (XmlNode node in nodeList)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                list.Add(node);
            }
        }
        return list;
    }

    private static ResourceRecord MapNodeToRecord(XmlNode node)
    {
        var record = new ResourceRecord
        {
            resourceType = node.Attributes?["type"]?.Value ?? node.Name,
            faculty = node.Attributes?["faculty"]?.Value
                      ?? node.ParentNode?.Attributes?["faculty"]?.Value
                      ?? string.Empty,
            department = node.Attributes?["department"]?.Value
                         ?? node.ParentNode?.Attributes?["department"]?.Value
                         ?? string.Empty
        };

        if (node.Attributes != null)
        {
            foreach (XmlAttribute attr in node.Attributes)
            {
                record.attributes[attr.Name] = attr.Value;
            }
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType != XmlNodeType.Element) continue;
            string text = child.InnerText.Trim();
            switch (child.Name)
            {
                case "Title":
                    record.resourceName = text;
                    break;
                case "Annotation":
                    record.annotation = text;
                    break;
                case "Author":
                    record.authorName = text;
                    break;
                case "Faculty":
                    record.faculty = text;
                    break;
                case "Department":
                    record.department = text;
                    break;
                case "UsageConditions":
                    record.usageConditions = text;
                    break;
                case "Address":
                    record.address = text;
                    break;
            }
        }

        return record;
    }
}
