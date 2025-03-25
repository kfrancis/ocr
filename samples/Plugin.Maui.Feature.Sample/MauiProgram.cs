using CommunityToolkit.Maui;
using Plugin.Maui.OCR;
using Plugin.Toolkit.Fonts.MaterialIcons;

namespace Plugin.Maui.Feature.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddMaterialIconsFonts();
            })
            .UseOcr();

        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
