using Plugin.Shared.OCR;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace Plugin.Maui.OCR;

partial class OcrImplementation : IOcrService
{
    /// <inheritdoc/>
    public Task InitAsync(CancellationToken ct = default)
    {
        // Windows OCR doesn't require explicit initialization.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<Shared.OCR.OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
    {
        var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages() ?? throw new NotSupportedException("OCR not supported on this device or no languages are installed.");

        using var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(imageData.AsBuffer());
        stream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(stream);
        var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

        var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

        var result = new Shared.OCR.OcrResult
        {
            AllText = ocrResult.Text,
            // Further process the result as needed, e.g., extract lines, words, etc.
        };

        return result;
    }
}
