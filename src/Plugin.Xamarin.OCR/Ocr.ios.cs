using System;
using Plugin.Shared.OCR;
using System.Threading.Tasks;
using System.Threading;

namespace Plugin.Xamarin.OCR;

partial class OcrImplementation : IOcrService
{
    public Task InitAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
