using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Xamarin.OCR
{
    internal class OcrImplementation : IOcrService
    {
        public IReadOnlyCollection<string> SupportedLanguages => throw new NotImplementedException();

        public event EventHandler<OcrCompletedEventArgs> RecognitionCompleted;

        public Task InitAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
