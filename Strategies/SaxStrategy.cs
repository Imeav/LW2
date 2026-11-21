using System.Xml;
using DepartmentResourcesApp.Models;
using XmlDataDocument = DepartmentResourcesApp.Models.XmlDataDocument;

namespace DepartmentResourcesApp.Strategies;

public class SaxStrategy : XmlAnalysisStrategy
{
    private readonly XmlReaderSettings settings = new()
    {
        IgnoreComments = true,
        IgnoreWhitespace = true
    };

    public override string Name => "SAX API (XmlReader)";

    public override List<ResourceRecord> Analyze(
        XmlDataDocument document,
        SearchParameters parameters,
        string elementName)
    {
        string path = document.GetFilePath();
        var results = new List<ResourceRecord>();
        if (string.IsNullOrWhiteSpace(path))
            return results;

        using var reader = XmlReader.Create(path, settings);

        string currentSectionDepartment = string.Empty;
        string currentSectionFaculty = string.Empty;
        ResourceRecord? currentRecord = null;
        string currentChildTag = string.Empty;
        bool skipCurrentResource = false;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Depth == 1)
                {
                    currentSectionDepartment = reader.GetAttribute("department") ?? string.Empty;
                    currentSectionFaculty = reader.GetAttribute("faculty") ?? string.Empty;
                }
                else if (reader.Depth == 2)
                {
                    bool typeFilterMatches = string.IsNullOrWhiteSpace(elementName) ||
                                             elementName == "Усі ресурси" ||
                                             string.Equals(reader.Name, elementName, StringComparison.Ordinal);

                    if (!typeFilterMatches)
                    {
                        skipCurrentResource = true;
                        currentRecord = null;
                        continue;
                    }

                    skipCurrentResource = false;
                    currentRecord = new ResourceRecord
                    {
                        resourceType = reader.GetAttribute("type") ?? reader.Name,
                        faculty = reader.GetAttribute("faculty") ?? currentSectionFaculty,
                        department = reader.GetAttribute("department") ?? currentSectionDepartment
                    };

                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            currentRecord.attributes[reader.Name] = reader.Value;
                        }
                        reader.MoveToElement();
                    }
                }
                else if (reader.Depth == 3 && currentRecord != null && !skipCurrentResource)
                {
                    currentChildTag = reader.Name;
                }
            }
            else if (reader.NodeType == XmlNodeType.Text && currentRecord != null && !skipCurrentResource)
            {
                string value = reader.Value.Trim();
                switch (currentChildTag)
                {
                    case "Title":
                        currentRecord.resourceName = value;
                        break;
                    case "Annotation":
                        currentRecord.annotation = value;
                        break;
                    case "Author":
                        currentRecord.authorName = value;
                        break;
                    case "Faculty":
                        currentRecord.faculty = value;
                        break;
                    case "Department":
                        currentRecord.department = value;
                        break;
                    case "UsageConditions":
                        currentRecord.usageConditions = value;
                        break;
                    case "Address":
                        currentRecord.address = value;
                        break;
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == 2)
            {
                if (currentRecord != null && !skipCurrentResource && MatchResource(currentRecord, parameters))
                {
                    results.Add(currentRecord);
                }
                currentRecord = null;
                currentChildTag = string.Empty;
                skipCurrentResource = false;
            }
        }

        return results;
    }

    public bool MatchResource(ResourceRecord record, SearchParameters parameters)
    {
        return record.Matches(parameters);
    }
}
