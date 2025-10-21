using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Plugin.Maui.OCR.Tests")]

namespace Plugin.Maui.OCR;

/// <summary>
///     Provides static access to Optical Character Recognition (OCR) services through a default implementation.
/// </summary>
/// <remarks>
///     The OcrPlugin class exposes a default OCR service via the <see cref="Default" /> property, allowing
///     consumers to perform OCR operations without explicitly instantiating an implementation. This class is intended for
///     scenarios where a shared, application-wide OCR service is sufficient. The default implementation can be replaced
///     internally using the <c>SetDefault</c> method, which is not accessible to external callers.
/// </remarks>
public static class OcrPlugin
{
    private static IOcrService? s_defaultImplementation;

    /// <summary>
    ///     Provides the default implementation for static usage of this API.
    /// </summary>
    public static IOcrService Default
    {
        get => s_defaultImplementation ??= new OcrImplementation();
    }

    /// <summary>
    ///     Sets the default implementation of the OCR service used by the application.
    /// </summary>
    /// <remarks>
    ///     This method is intended for internal configuration and should be used with caution, as
    ///     changing the default implementation may affect OCR operations throughout the application.
    /// </remarks>
    /// <param name="implementation">
    ///     The OCR service implementation to use as the default. Specify <see langword="null" /> to clear the current
    ///     default.
    /// </param>
    internal static void SetDefault(IOcrService? implementation)
    {
        s_defaultImplementation = implementation;
    }
}
