using DepartmentResourcesApp.Models;

namespace DepartmentResourcesApp.Strategies;

public abstract class XmlAnalysisStrategy
{
    public abstract string Name { get; }

    public abstract List<ResourceRecord> Analyze(
        XmlDataDocument document,
        SearchParameters parameters,
        string elementName);
}
