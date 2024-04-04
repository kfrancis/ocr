namespace Plugin.Maui.OCR;

/// <summary>
/// OCR API.
/// </summary>
public static class OcrPlugin
{
    private static IOcrService? s_defaultImplementation;

    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    public static IOcrService Default =>
        s_defaultImplementation ??= new OcrImplementation();

    /// <summary>
    /// Sets the default implementation.
    /// </summary>
    /// <param name="implementation"></param>
    internal static void SetDefault(IOcrService? implementation) =>
        s_defaultImplementation = implementation;
}
