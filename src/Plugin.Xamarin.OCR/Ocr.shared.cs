using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Shared.OCR;

namespace Plugin.Xamarin.OCR;

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
