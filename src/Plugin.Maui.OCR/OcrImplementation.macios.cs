using CoreGraphics;
using Foundation;
using UIKit;
using Vision;

namespace Plugin.Maui.OCR;

class OcrImplementation : IOcrService
{
    private static readonly object s_initLock = new();
    private bool _isInitialized;
    private IReadOnlyCollection<string> _supportedLanguages;

    public IReadOnlyCollection<string> SupportedLanguages => _supportedLanguages;

    /// <summary>
    /// Initialize the OCR on the platform
    /// </summary>
    /// <param name="ct">An optional cancellation token</param>
    public Task InitAsync(CancellationToken ct = default)
    {
        lock (s_initLock)
        {
            if (_isInitialized) return Task.CompletedTask;
            _isInitialized = true;
        }

        var tcs = new TaskCompletionSource<OcrResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        ct.Register(() => tcs.TrySetCanceled());
        using var recognizeTextRequest = new VNRecognizeTextRequest((_, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.LocalizedDescription));
                return;
            }

            if (ct.IsCancellationRequested)
            {
                tcs.TrySetCanceled(ct);
            }
        });
        var supportedLangs = GetSupportedRecognitionLanguages(recognizeTextRequest, recognizeTextRequest.RecognitionLevel, out var langError);
        if (langError != null)
        {
            throw new InvalidOperationException(langError.LocalizedDescription);
        }
        _supportedLanguages = new List<string>(supportedLangs.Select(ns => ns)).AsReadOnly();

        return Task.CompletedTask;
    }

    private static OcrResult ProcessRecognitionResults(VNRequest request, CGSize imageSize)
    {
        var ocrResult = new OcrResult();

        var observations = request.GetResults<VNRecognizedTextObservation>();
        if (observations == null || observations.Length == 0)
        {
            ocrResult.Success = false;
            return ocrResult;
        }

        foreach (var observation in observations)
        {
            var topCandidate = observation.TopCandidates(1).FirstOrDefault();
            if (topCandidate != null)
            {
                ocrResult.AllText += " " + topCandidate.String;
                ocrResult.Lines.Add(topCandidate.String);

                if (!string.IsNullOrEmpty(topCandidate.String))
                {
                    var textRange = new NSRange(0, topCandidate.String.Length);
                    using var box = topCandidate.GetBoundingBox(textRange, out var boxError);
                    if (boxError != null)
                    {
                        throw new InvalidOperationException(boxError.LocalizedDescription);
                    }

                    var boxRect = ConvertToImageRect(box, imageSize);

                    // Splitting by spaces to create elements might not be accurate for all languages/scripts
                    topCandidate.String.Split(" ").ToList().ForEach(e => ocrResult.Elements.Add(new OcrResult.OcrElement
                    {
                        Text = e,
                        Confidence = topCandidate.Confidence,
                        X = Convert.ToInt32(boxRect.X),
                        Y = Convert.ToInt32(boxRect.Y),
                        Width = Convert.ToInt32(boxRect.Width),
                        Height = Convert.ToInt32(boxRect.Height)
                    }));
                }
                else
                {
                    // Splitting by spaces to create elements might not be accurate for all languages/scripts
                    topCandidate.String.Split(" ").ToList().ForEach(e => ocrResult.Elements.Add(new OcrResult.OcrElement
                    {
                        Text = e,
                        Confidence = topCandidate.Confidence
                    }));
                }
            }
        }

        ocrResult.Success = true;
        return ocrResult;
    }

    private static UIImage? ImageFromByteArray(byte[] data)
    {
        return data != null ? new UIImage(NSData.FromArray(data)) : null;
    }

    private static Rect ConvertToImageRect(VNRectangleObservation boundingBox, CGSize imageSize)
    {
        var topLeft = NormalizePoint(boundingBox.TopLeft, imageSize);
        var bottomRight = NormalizePoint(boundingBox.BottomRight, imageSize);

        // Flip it for top left (0,0) image coordinates
        return new Rect(
            topLeft.X,
            imageSize.Height - topLeft.Y,
            Math.Abs(bottomRight.X - topLeft.X),
            Math.Abs(topLeft.Y - bottomRight.Y)
        );
    }

    private static Point NormalizePoint(CGPoint point, CGSize imageSize)
    {
        return new Point(
            point.X * imageSize.Width,
            point.Y * imageSize.Height
        );
    }

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="tryHard">True to try and tell the API to be more accurate, otherwise just be fast.</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default)
    {
        return await RecognizeTextAsync(imageData, new OcrOptions(TryHard: tryHard), ct);
    }

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="options">The options for OCR</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(InitAsync)} must be called before {nameof(RecognizeTextAsync)}.");
        }

        ct.ThrowIfCancellationRequested();

        var tcs = new TaskCompletionSource<OcrResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        ct.Register(() => tcs.TrySetCanceled());

        try
        {
            using var image = ImageFromByteArray(imageData) ?? throw new ArgumentException("Invalid image data");
            var imageSize = image.Size;

            using var recognizeTextRequest = new VNRecognizeTextRequest((request, error) =>
            {
                if (error != null)
                {
                    tcs.TrySetException(new Exception(error.LocalizedDescription));
                    return;
                }

                if (ct.IsCancellationRequested)
                {
                    tcs.TrySetCanceled(ct);
                    return;
                }

                var result = ProcessRecognitionResults(request, imageSize);
                tcs.TrySetResult(result);
            });

            switch (options.TryHard)
            {
                case true:
                    recognizeTextRequest.RecognitionLevel = VNRequestTextRecognitionLevel.Accurate;
                    break;

                case false:
                    recognizeTextRequest.RecognitionLevel = VNRequestTextRecognitionLevel.Fast;
                    break;
            }

            if (!string.IsNullOrEmpty(options.Language))
            {
                var supportedLangs = GetSupportedRecognitionLanguages(recognizeTextRequest, recognizeTextRequest.RecognitionLevel, out var langError);
                if (langError != null)
                {
                    throw new InvalidOperationException(langError.LocalizedDescription);
                }
                var supportedLangList = new List<string>(supportedLangs.Select(ns => ns));

                if (options.Language is string langString && supportedLangList.Contains(langString))
                {
                    recognizeTextRequest.RecognitionLanguages = new[] { langString };
                }
                else
                {
                    throw new NotSupportedException($"Unsupported language \"{options.Language}\". Supported languages are: ({string.Join(",", supportedLangList)})");
                }
            }

            recognizeTextRequest.UsesLanguageCorrection = options.TryHard;
            recognizeTextRequest.UsesCpuOnly = false;
            recognizeTextRequest.PreferBackgroundProcessing = true;
            recognizeTextRequest.MinimumTextHeight = 0;

            using var ocrHandler = new VNImageRequestHandler(image.CGImage, new NSDictionary());
            ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out var error);
            if (error != null)
            {
                throw new InvalidOperationException(error.LocalizedDescription);
            }
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return await tcs.Task;
    }

    private static IEnumerable<string> GetSupportedRecognitionLanguages(VNRecognizeTextRequest recognizeTextRequest, VNRequestTextRecognitionLevel textRecognitionLevel, out NSError? langError)
    {
        if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacOSVersionAtLeast(12))
        {
            // New instance method available from iOS 15.0, macOS 12.0
            return recognizeTextRequest.GetSupportedRecognitionLanguages(out langError).Select(ns => (string)ns);
        }
        else if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
        {
            // Static method available until iOS 14.9, macOS 11.9
            return VNRecognizeTextRequest.GetSupportedRecognitionLanguages(textRecognitionLevel, VNRecognizeTextRequestRevision.Unspecified, out langError) ?? Array.Empty<string>();
        }
        else
        {
            // Default case when no suitable API is available
            langError = null;
            return Array.Empty<string>();
        }
    }
}
