using Microsoft.Maui.Controls;

namespace OcrSample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
