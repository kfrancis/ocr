using Plugin.Shared.OCR;

namespace Plugin.Maui.OCR;

partial class OcrImplementation : IOcrService
{
    public Task InitAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    // TODO Implement your .NET specific code.
    // This usually is a placeholder as .NET MAUI apps typically don't run on .NET generic targets unless through unit tests and such
    public Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}