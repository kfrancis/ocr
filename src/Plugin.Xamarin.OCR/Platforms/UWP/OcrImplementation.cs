using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using static Plugin.Xamarin.OCR.OcrResult;

namespace Plugin.Xamarin.OCR.Platforms.UWP
{
    internal class OcrImplementation : IOcrService
    {
        /// <summary>
        /// Initialize the OCR on the platform
        /// </summary>
        /// <param name="ct">An optional cancellation token</param>
        public Task InitAsync(CancellationToken ct = default)
        {
            // Windows OCR doesn't require explicit initialization.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Takes an image and returns the text found in the image.
        /// </summary>
        /// <param name="imageData">The image data</param>
        /// <param name="ct">An optional cancellation token</param>
        /// <returns>The OCR result</returns>
        public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
        {
            var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages() ?? throw new NotSupportedException("OCR not supported on this device or no languages are installed.");

            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(imageData.AsBuffer());
            stream.Seek(0);

            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

            var result = new OcrResult
            {
                AllText = ocrResult.Text,
                Success = true,
                Elements = new List<OcrElement>(),
                Lines = new List<string>()
            };

            foreach (var line in ocrResult.Lines)
            {
                result.Lines.Add(line.Text);
                foreach (var word in line.Words)
                {
                    result.Elements.Add(new OcrElement
                    {
                        Text = word.Text,
                        X = (int)Math.Truncate(word.BoundingRect.X),
                        Y = (int)Math.Truncate(word.BoundingRect.Y),
                        Width = (int)Math.Truncate(word.BoundingRect.Width),
                        Height = (int)Math.Truncate(word.BoundingRect.Height)
                    });
                }
            }

            return result;
        }
    }
}
