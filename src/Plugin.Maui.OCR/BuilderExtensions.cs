using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.OCR;

public static class OcrServiceExtensions
{
    public static MauiAppBuilder UseOcr(this MauiAppBuilder builder)
    {
        // Register the IOcrService implementation with the DI container.
        // This ensures that whenever IOcrService is injected, the specific platform implementation is provided.
        builder.Services.AddSingleton<IOcrService, OcrImplementation>();

        return builder;
    }
}
