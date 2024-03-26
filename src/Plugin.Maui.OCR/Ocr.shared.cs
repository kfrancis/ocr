using Plugin.Shared.OCR;

namespace Plugin.Maui.OCR;

public static class OCR
{
    static IOcrService? s_defaultImplementation;

    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    public static IOcrService Default =>
        s_defaultImplementation ??= new OcrImplementation();

    internal static void SetDefault(IOcrService? implementation) =>
        s_defaultImplementation = implementation;
}
