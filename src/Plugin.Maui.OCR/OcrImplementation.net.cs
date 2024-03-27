using Plugin.Shared.OCR;

namespace Plugin.Maui.OCR;

// This usually is a placeholder as .NET MAUI apps typically don't run on .NET generic targets unless through unit tests and such
partial class OcrImplementation : IOcrService
{
    /// <inheritdoc/>
    public Task InitAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
