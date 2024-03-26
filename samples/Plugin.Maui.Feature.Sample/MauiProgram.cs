using Plugin.Shared.OCR;
using Plugin.Maui.OCR;

namespace Plugin.Maui.Feature.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseOcr();

        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
