using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Shared.OCR;

namespace Plugin.Xamarin.OCR.iOS
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
