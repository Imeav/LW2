namespace DepartmentResourcesApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        window.Destroying += (s, e) =>
        {
            if (window.Page is Shell shell && shell.CurrentPage is MainPage mainPage)
            {
                mainPage.ConfirmExit();
            }
        };

        return window;
    }
}
