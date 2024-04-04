using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Xamarin.OCR
{
    internal class OcrImplementation : IOcrService
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
