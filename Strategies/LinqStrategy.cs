using System.Xml.Linq;
using DepartmentResourcesApp.Models;

namespace DepartmentResourcesApp.Strategies;

public class LinqStrategy : XmlAnalysisStrategy
{
    private XDocument xmlDocument = new();

    public override string Name => "LINQ to XML (XDocument)";

    public override List<ResourceRecord> Analyze(
        XmlDataDocument document,
        SearchParameters parameters,
        string elementName)
    {
        string path = document.GetFilePath();
        if (string.IsNullOrWhiteSpace(path))
            return new List<ResourceRecord>();

        xmlDocument = XDocument.Load(path);

        IEnumerable<XElement> query = BuildQuery(parameters);

        if (!string.IsNullOrWhiteSpace(elementName) && elementName != "Усі ресурси")
        {
            query = query.Where(el => string.Equals(el.Name.LocalName, elementName, StringComparison.Ordinal));
        }

        return query
            .Select(MapElementToRecord)
            .Where(record => record.Matches(parameters))
            .ToList();
    }

    public IEnumerable<XElement> BuildQuery(SearchParameters parameters)
    {
        var elements = xmlDocument.Root?.Elements().Elements() ?? Enumerable.Empty<XElement>();

        if (parameters != null &&
            !string.IsNullOrWhiteSpace(parameters.attributeName) &&
            parameters.attributeName != "Усі атрибути")
        {
            elements = elements.Where(el =>
            {
                var attr = el.Attribute(parameters.attributeName);
                if (attr == null) return false;

                if (!string.IsNullOrWhiteSpace(parameters.attributeValue) &&
                    parameters.attributeValue != "Усі значення")
                {
                    return string.Equals(attr.Value, parameters.attributeValue, StringComparison.OrdinalIgnoreCase);
                }

                return true;
            });
        }

        return elements;
    }

    private static ResourceRecord MapElementToRecord(XElement el)
    {
        var record = new ResourceRecord
        {
            resourceType = el.Attribute("type")?.Value ?? el.Name.LocalName,
            resourceName = el.Element("Title")?.Value.Trim() ?? string.Empty,
            annotation = el.Element("Annotation")?.Value.Trim() ?? string.Empty,
            authorName = el.Element("Author")?.Value.Trim() ?? string.Empty,
            faculty = el.Element("Faculty")?.Value.Trim()
                      ?? el.Attribute("faculty")?.Value
                      ?? el.Parent?.Attribute("faculty")?.Value
                      ?? string.Empty,
            department = el.Element("Department")?.Value.Trim()
                         ?? el.Attribute("department")?.Value
                         ?? el.Parent?.Attribute("department")?.Value
                         ?? string.Empty,
            usageConditions = el.Element("UsageConditions")?.Value.Trim() ?? string.Empty,
            address = el.Element("Address")?.Value.Trim() ?? string.Empty
        };

        foreach (var attr in el.Attributes())
        {
            record.attributes[attr.Name.LocalName] = attr.Value;
        }

        return record;
    }
}
