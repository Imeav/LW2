namespace DepartmentResourcesApp.Models;

public class SearchParameters
{
    public string attributeName { get; set; } = string.Empty;
    public string attributeValue { get; set; } = string.Empty;
    public string keyword { get; set; } = string.Empty;

    public bool Validate()
    {
        bool hasAttributeFilter = !string.IsNullOrWhiteSpace(attributeName) &&
                                  attributeName != "Усі атрибути";
        bool hasValueFilter = !string.IsNullOrWhiteSpace(attributeValue) &&
                              attributeValue != "Усі значення";

        if (hasValueFilter && !hasAttributeFilter)
            return false;

        return true;
    }

    public string BuildXPathQuery(string elementName)
    {
        string targetNode = string.IsNullOrWhiteSpace(elementName) || elementName == "Усі ресурси"
            ? "/*/*/*"
            : $"/*/*/{elementName}";

        var predicates = new List<string>();

        if (!string.IsNullOrWhiteSpace(attributeName) && attributeName != "Усі атрибути")
        {
            if (!string.IsNullOrWhiteSpace(attributeValue) && attributeValue != "Усі значення")
            {
                string escapedValue = attributeValue.Replace("'", "&apos;");
                predicates.Add($"@{attributeName}='{escapedValue}'");
            }
            else
            {
                predicates.Add($"@{attributeName}");
            }
        }

        return predicates.Count > 0
            ? $"{targetNode}[{string.Join(" and ", predicates)}]"
            : targetNode;
    }

    public string BuildQueryDescription()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(attributeName) && attributeName != "Усі атрибути")
        {
            string val = !string.IsNullOrWhiteSpace(attributeValue) && attributeValue != "Усі значення"
                ? $" = \"{attributeValue}\""
                : " (наявність атрибута)";
            parts.Add($"Атрибут [{attributeName}]{val}");
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            parts.Add($"Ключове слово: \"{keyword}\"");
        }

        return parts.Count > 0
            ? string.Join("; ", parts)
            : "Усі інформаційні ресурси без обмежень";
    }
}
