using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Shared.OCR;

namespace Plugin.Xamarin.OCR.Android
{
    public class OcrImplementation : IOcrService
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
}
