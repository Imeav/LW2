using System.Text;

namespace DepartmentResourcesApp.Models;

public class ResourceRecord
{
    public string resourceType { get; set; } = string.Empty;
    public string resourceName { get; set; } = string.Empty;
    public string annotation { get; set; } = string.Empty;
    public string authorName { get; set; } = string.Empty;
    public string faculty { get; set; } = string.Empty;
    public string department { get; set; } = string.Empty;
    public string usageConditions { get; set; } = string.Empty;
    public string address { get; set; } = string.Empty;
    public Dictionary<string, string> attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string DisplayTitle => string.IsNullOrWhiteSpace(resourceName) ? "(Без назви)" : resourceName;
    public string DisplaySubtitle => $"Вид: {resourceType} | Автор: {authorName}";
    public string DisplayDetails => ToDisplayString();

    public string ToDisplayString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Назва ресурсу: {resourceName}");
        sb.AppendLine($"Вид: {resourceType}");
        sb.AppendLine($"П.І.П. автора: {authorName}");
        sb.AppendLine($"Факультет: {faculty}");
        sb.AppendLine($"Кафедра: {department}");
        sb.AppendLine($"Анотація: {annotation}");
        sb.AppendLine($"Умови використання: {usageConditions}");
        sb.AppendLine($"Адреса: {address}");

        if (attributes.Count > 0)
        {
            var attrs = string.Join(", ", attributes.Select(kv => $"{kv.Key}=\"{kv.Value}\""));
            sb.Append($"Атрибути вузла: {attrs}");
        }

        return sb.ToString().TrimEnd();
    }

    public bool Matches(SearchParameters parameters)
    {
        if (parameters == null)
            return true;

        if (!string.IsNullOrWhiteSpace(parameters.attributeName) &&
            parameters.attributeName != "Усі атрибути")
        {
            if (!attributes.TryGetValue(parameters.attributeName, out var actualValue))
                return false;

            if (!string.IsNullOrWhiteSpace(parameters.attributeValue) &&
                parameters.attributeValue != "Усі значення" &&
                !string.Equals(actualValue, parameters.attributeValue, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(parameters.keyword))
        {
            var kw = parameters.keyword.Trim();
            bool foundInFields =
                resourceType.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                resourceName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                annotation.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                authorName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                faculty.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                department.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                usageConditions.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                address.Contains(kw, StringComparison.OrdinalIgnoreCase);

            bool foundInAttributes = attributes.Any(a =>
                a.Key.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                a.Value.Contains(kw, StringComparison.OrdinalIgnoreCase));

            if (!foundInFields && !foundInAttributes)
                return false;
        }

        return true;
    }
}
