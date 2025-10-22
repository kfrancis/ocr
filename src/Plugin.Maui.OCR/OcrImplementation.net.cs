using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Maui.OCR;

// This usually is a placeholder as .NET MAUI apps typically don't run on .NET generic targets unless through unit tests and such
public class OcrImplementation : IOcrService
{
    public IReadOnlyCollection<string> SupportedLanguages => throw new NotImplementedException();

    public event EventHandler<OcrCompletedEventArgs> RecognitionCompleted;

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
    /// <param name="tryHard">No effect.</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default)
    {
        return await RecognizeTextAsync(imageData, new OcrOptions.Builder().SetTryHard(tryHard).Build(), ct);
    }

    public Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
    {
        RecognitionCompleted.Invoke(this, new OcrCompletedEventArgs(null));
        return Task.CompletedTask;
    }
}
