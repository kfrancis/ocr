namespace Plugin.Maui.OCR;

// This usually is a placeholder as .NET MAUI apps typically don't run on .NET generic targets unless through unit tests and such
partial class OcrImplementation : IOcrService
{
    /// <summary>
    /// Initialize the OCR on the platform
    /// </summary>
    /// <param name="ct">An optional cancellation token</param>
    public Task InitAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    public Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
