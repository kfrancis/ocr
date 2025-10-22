using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.OCR;

/// <summary>
///     Provides extension methods for configuring OCR services in a .NET MAUI application.
/// </summary>
/// <remarks>
///     Use this class to register OCR service dependencies with the application's dependency injection
///     container. The extension methods ensure that the OCR service is registered only once, even if called multiple times
///     during application setup.
/// </remarks>
public static class OcrServiceExtensions
{
    /// <summary>
    ///     Configures the application to use optical character recognition (OCR) services by registering the default OCR
    ///     implementation.
    /// </summary>
    /// <remarks>
    ///     This method registers the OCR service only if it has not already been added, ensuring
    ///     idempotent configuration. Call this method during app startup to enable OCR functionality throughout the
    ///     application.
    /// </remarks>
    /// <param name="builder">The application builder used to configure services and middleware for the MAUI app.</param>
    /// <returns>The same <paramref name="builder" /> instance, enabling method chaining.</returns>
    public static MauiAppBuilder UseOcr(this MauiAppBuilder builder)
    {
        // Register IOcrService only once (idempotent) even if UseOcr is called multiple times.
        if (builder.Services.All(sd => sd.ServiceType != typeof(IOcrService)))
        {
            builder.Services.AddSingleton<IOcrService, OcrImplementation>();
        }

        return builder;
    }
}
