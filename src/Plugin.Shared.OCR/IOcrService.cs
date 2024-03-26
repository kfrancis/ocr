using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Shared.OCR
{
    public interface IOcrService
    {
        Task InitAsync(CancellationToken ct = default);
        Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default);
    }

    public class OcrResult
    {
        public bool Success { get; set; }

        public string AllText { get; set; }

        public IList<OcrElement> Elements { get; set; } = new List<OcrElement>();
        public IList<string> Lines { get; set; } = new List<string>();

        public class OcrElement
        {
            public string Text { get; set; }
            public float Confidence { get; set; }
        }
    }
}
