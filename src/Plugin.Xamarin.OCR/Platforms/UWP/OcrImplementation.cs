using System;
using System.Collections.Generic;
using System.Linq;
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
        private IReadOnlyCollection<string> _supportedLanguages;
        public IReadOnlyCollection<string> SupportedLanguages => _supportedLanguages;

        public event EventHandler<OcrCompletedEventArgs> RecognitionCompleted;

        /// <summary>
        /// Initialize the OCR on the platform
        /// </summary>
        /// <param name="ct">An optional cancellation token</param>
        public Task InitAsync(CancellationToken ct = default)
        {
            _supportedLanguages = OcrEngine.AvailableRecognizerLanguages.Select(x => x.LanguageTag).ToList().AsReadOnly();

            return Task.CompletedTask;
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
            return await RecognizeTextAsync(imageData, new OcrOptions(tryHard: tryHard, patternConfig: null), ct);
        }

        public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(options.Language) && !OcrEngine.IsLanguageSupported(new Windows.Globalization.Language(options.Language)))
            {
                throw new NotSupportedException($"Unsupported language \"{options.Language}\". Supported languages are: ({string.Join(",", OcrEngine.AvailableRecognizerLanguages.Select(x => x.LanguageTag))})");
            }

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

            if (options.PatternConfigs != null)
            {
                foreach (var config in options.PatternConfigs)
                {
                    var match = OcrPatternMatcher.ExtractPattern(result.AllText, config);
                    if (!string.IsNullOrEmpty(match))
                    {
                        result.MatchedValues.Add(match);
                    }
                }
            }

            options.CustomCallback?.Invoke(result.AllText);

            return result;
        }

        public async Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(options.Language) && !OcrEngine.IsLanguageSupported(new Windows.Globalization.Language(options.Language)))
            {
                throw new NotSupportedException($"Unsupported language \"{options.Language}\". Supported languages are: ({string.Join(",", OcrEngine.AvailableRecognizerLanguages.Select(x => x.LanguageTag))})");
            }

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

            RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(result, null));
        }
    }
}
