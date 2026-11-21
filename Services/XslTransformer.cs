using System.Xml.Xsl;

namespace DepartmentResourcesApp.Services;

public class XslTransformer
{
    private readonly XslCompiledTransform engine = new();

    public void TransformToHtml(string xmlPath, string xslPath, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            throw new FileNotFoundException("Вхідний XML-файл не знайдено.", xmlPath);

        if (string.IsNullOrWhiteSpace(xslPath) || !File.Exists(xslPath))
            throw new FileNotFoundException("XSL-шаблон не знайдено.", xslPath);

        engine.Load(xslPath);
        engine.Transform(xmlPath, outputPath);
    }
}
