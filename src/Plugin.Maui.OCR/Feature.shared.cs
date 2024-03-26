using Plugin.Shared.OCR;

namespace Plugin.Maui.OCR;

public static class Feature
{
    static IOcrService? defaultImplementation;

    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    public static IOcrService Default =>
        defaultImplementation ??= new OcrImplementation();

    internal static void SetDefault(IOcrService? implementation) =>
        defaultImplementation = implementation;
}
