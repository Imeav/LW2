using DepartmentResourcesApp.Models;
using DepartmentResourcesApp.Services;
using DepartmentResourcesApp.Strategies;

namespace DepartmentResourcesApp;

public partial class MainPage : ContentPage
{
    private readonly XmlDataDocument xmlDocument = new();
    private readonly XslTransformer transformer = new();
    private readonly List<XmlAnalysisStrategy> strategies;
    private XmlAnalysisStrategy selectedStrategy;

    private SearchParameters currentParameters = new();
    private string selectedXslPath = string.Empty;
    private string selectedOutputHtmlPath = string.Empty;

    public MainPage()
    {
        InitializeComponent();

        strategies = new List<XmlAnalysisStrategy>
        {
            new DomStrategy(),
            new SaxStrategy(),
            new LinqStrategy()
        };

        selectedStrategy = strategies[0];
        StrategyPicker.ItemsSource = strategies.Select(s => s.Name).ToList();
        StrategyPicker.SelectedIndex = 0;
    }

    private void OnOpenXmlClicked(object? sender, EventArgs e) => OpenXmlFile();
    private void OnSearchClicked(object? sender, EventArgs e) => SearchResources();
    private void OnTransformHtmlClicked(object? sender, EventArgs e) => TransformXmlToHtml();
    private void OnClearClicked(object? sender, EventArgs e) => Clear();
    private void OnHelpClicked(object? sender, EventArgs e) => ShowHelp();
    private void OnAboutClicked(object? sender, EventArgs e) => ShowAbout();
    private void OnExitClicked(object? sender, EventArgs e) => ConfirmExit();

    private void OnStrategyChanged(object? sender, EventArgs e)
    {
        if (StrategyPicker.SelectedItem is string methodName)
        {
            SelectAnalysisMethod(methodName);
        }
    }

    private void OnAttributeNameChanged(object? sender, EventArgs e)
    {
        LoadAttributeValues();
    }

    public async void OpenXmlFile()
    {
        try
        {
            var xmlFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".xml" } },
                { DevicePlatform.macOS, new[] { "xml", "public.xml" } },
                { DevicePlatform.Android, new[] { "text/xml", "application/xml" } },
                { DevicePlatform.iOS, new[] { "public.xml" } }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XML-файл інформаційних ресурсів кафедри",
                FileTypes = xmlFileType
            });

            if (result == null)
                return;

            xmlDocument.Load(result.FullPath);
            LoadAvailableResources();
            LoadAttributeValues();

            StatusLabel.Text = $"Завантажено файл: {Path.GetFileName(result.FullPath)}";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка відкриття XML", ex.Message, "ОК");
        }
    }

    public void LoadAvailableResources()
    {
        var elements = new List<string> { "Усі ресурси" };
        elements.AddRange(xmlDocument.GetResourceElementNames());
        ElementPicker.ItemsSource = elements;
        ElementPicker.SelectedIndex = 0;

        var attributes = new List<string> { "Усі атрибути" };
        attributes.AddRange(xmlDocument.GetAttributeNames());
        AttributeNamePicker.ItemsSource = attributes;
        AttributeNamePicker.SelectedIndex = 0;
    }

    public void LoadAttributeValues()
    {
        var values = new List<string> { "Усі значення" };
        if (AttributeNamePicker.SelectedItem is string attrName && attrName != "Усі атрибути")
        {
            values.AddRange(xmlDocument.GetAttributeValues(attrName));
        }
        AttributeValuePicker.ItemsSource = values;
        AttributeValuePicker.SelectedIndex = 0;
    }

    public void SetSearchParameters()
    {
        currentParameters = new SearchParameters
        {
            attributeName = AttributeNamePicker.SelectedItem as string ?? string.Empty,
            attributeValue = AttributeValuePicker.SelectedItem as string ?? string.Empty,
            keyword = KeywordEntry.Text ?? string.Empty
        };
    }

    public void SelectAnalysisMethod(string method)
    {
        var found = strategies.FirstOrDefault(s => s.Name == method);
        if (found != null)
        {
            selectedStrategy = found;
            StatusLabel.Text = $"Обрано метод аналізу: {selectedStrategy.Name}";
        }
    }

    public async void SearchResources()
    {
        if (string.IsNullOrWhiteSpace(xmlDocument.GetFilePath()))
        {
            await DisplayAlertAsync("Увага", "Спочатку завантажте XML-файл.", "ОК");
            return;
        }

        SetSearchParameters();

        if (!currentParameters.Validate())
        {
            await DisplayAlertAsync("Некоректні параметри", "Оберіть назву атрибута перед вибором його значення.", "ОК");
            return;
        }

        string elementName = ElementPicker.SelectedItem as string ?? "Усі ресурси";
        var results = selectedStrategy.Analyze(xmlDocument, currentParameters, elementName);

        DisplayResults(results);
        StatusLabel.Text = $"[{selectedStrategy.Name}] Знайдено записів: {results.Count}. Запит: {currentParameters.BuildQueryDescription()}";
    }

    public void DisplayResults(List<ResourceRecord> results)
    {
        ResultsCollectionView.ItemsSource = null;
        ResultsCollectionView.ItemsSource = results;
    }

    public string SelectXslFile()
    {
        return selectedXslPath;
    }

    public string SaveHtmlFile()
    {
        return selectedOutputHtmlPath;
    }

    public async void TransformXmlToHtml()
    {
        try
        {
            string xmlPath = xmlDocument.GetFilePath();
            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                await DisplayAlertAsync("Увага", "Спочатку завантажте XML-файл.", "ОК");
                return;
            }

            var xslFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".xsl", ".xslt" } },
                { DevicePlatform.macOS, new[] { "xsl", "xslt" } },
                { DevicePlatform.Android, new[] { "text/xml", "application/xslt+xml" } },
                { DevicePlatform.iOS, new[] { "public.xml" } }
            });

            var pickedXsl = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XSL-шаблон для трансформації у HTML",
                FileTypes = xslFileType
            });

            if (pickedXsl == null)
                return;

            selectedXslPath = pickedXsl.FullPath;
            string xslPath = SelectXslFile();

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string targetDir = !string.IsNullOrWhiteSpace(desktop) && Directory.Exists(desktop)
                ? desktop
                : FileSystem.AppDataDirectory;

            selectedOutputHtmlPath = Path.Combine(
                targetDir,
                $"DepartmentResources_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            string outputPath = SaveHtmlFile();

            transformer.TransformToHtml(xmlPath, xslPath, outputPath);
            StatusLabel.Text = $"HTML успішно збережено: {outputPath}";

            bool openNow = await DisplayAlertAsync(
                "Трансформація завершена",
                $"Файл збережено за шляхом:\n{outputPath}\n\nВідкрити його у браузері?",
                "Так",
                "Ні");

            if (openNow)
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest("HTML-звіт", new ReadOnlyFile(outputPath)));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка XSLT-перетворення", ex.Message, "ОК");
        }
    }

    public void Clear()
    {
        if (ElementPicker.ItemsSource != null) ElementPicker.SelectedIndex = 0;
        if (AttributeNamePicker.ItemsSource != null) AttributeNamePicker.SelectedIndex = 0;
        if (AttributeValuePicker.ItemsSource != null) AttributeValuePicker.SelectedIndex = 0;
        KeywordEntry.Text = string.Empty;
        ResultsCollectionView.ItemsSource = null;
        StatusLabel.Text = "Параметри пошуку та результати очищено.";
    }

    public async void ShowHelp()
    {
        await DisplayAlertAsync(
            "Довідка користувача",
            "1. Натисніть «Відкрити XML» та виберіть файл зі структурою інформаційних ресурсів кафедри.\n" +
            "2. Оберіть спосіб аналізу XML: DOM API, SAX API або LINQ to XML.\n" +
            "3. За потреби вкажіть тип вузла, атрибут, його значення або ключове слово та натисніть «Пошук».\n" +
            "4. Для експорту таблиці у HTML натисніть «Перетворити XML у HTML» та вкажіть XSL-файл.",
            "Зрозуміло");
    }

    public async void ShowAbout()
    {
        await DisplayAlertAsync(
            "Про програму",
            "Лабораторна робота № 2\n" +
            "Варіант № 7: «Інформаційні ресурси кафедри»\n" +
            "Технології: .NET MAUI, DOM, SAX, LINQ to XML, XSLT\n" +
            "Архітектурний шаблон: Strategy",
            "ОК");
    }

    public bool ConfirmExit()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            bool answer = await DisplayAlertAsync(
                "Підтвердження виходу",
                "Чи дійсно ви хочете завершити роботу з програмою?",
                "Так",
                "Ні");

            if (answer)
            {
                Application.Current?.Quit();
            }
        });
        return true;
    }
}
