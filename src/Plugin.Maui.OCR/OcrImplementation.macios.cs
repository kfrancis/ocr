using CoreGraphics;
using Foundation;
using UIKit;
using Vision;

namespace Plugin.Maui.OCR;

partial class OcrImplementation : IOcrService
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

        // Perform any necessary initialization here.
        // Example: Loading models, setting up resources, etc.
        if (OperatingSystem.IsIOSVersionAtLeast(14, 2) || OperatingSystem.IsMacOSVersionAtLeast(14, 0, 0))
        {
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
            var supportedLangs = recognizeTextRequest.GetSupportedRecognitionLanguages(out var langError);
            if (langError != null)
            {
                throw new Exception(langError.LocalizedDescription);
            }
            _supportedLanguages = new List<string>(supportedLangs.Select(ns => (string)ns)).AsReadOnly();
        }

        return Task.CompletedTask;
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

                // Convert the normalized CGRect to image coordinates
                var boundingBox = observation.BoundingBox;
                var x = (int)(boundingBox.X * imageSize.Width);
                var y = (int)((1 - boundingBox.Y - boundingBox.Height) * imageSize.Height); // flip the Y coordinate
                var width = (int)(boundingBox.Width * imageSize.Width);
                var height = (int)(boundingBox.Height * imageSize.Height);

                // Splitting by spaces to create elements might not be accurate for all languages/scripts
                topCandidate.String.Split(" ").ToList().ForEach(e => ocrResult.Elements.Add(new OcrResult.OcrElement
                {
                    Text = e,
                    Confidence = topCandidate.Confidence,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                }));
            }
        }

        ocrResult.Success = true;
        return ocrResult;
    }

    private static UIImage? ImageFromByteArray(byte[] data)
    {
        return data != null ? new UIImage(NSData.FromArray(data)) : null;
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

            // for ios/macos 14.2 or later
            if ((!string.IsNullOrEmpty(options.Language) && OperatingSystem.IsIOSVersionAtLeast(14, 2)) || OperatingSystem.IsMacOSVersionAtLeast(10, 15, 2))
            {
                var supportedLangs = recognizeTextRequest.GetSupportedRecognitionLanguages(out var langError);
                if (langError != null)
                {
                    throw new Exception(langError.LocalizedDescription);
                }
                var supportedLangList = new List<string>(supportedLangs.Select(ns => (string)ns));

                if (options.Language is string langString && supportedLangList.Contains(langString))
                {
                    recognizeTextRequest.RecognitionLanguages = new[] { langString };
                }
                else
                {
                    throw new ArgumentException($"Unsupported language \"{options.Language}\". Supported languages are: ({string.Join(",", supportedLangList)})", nameof(options.Language));
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
                throw new Exception(error.LocalizedDescription);
            }
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return await tcs.Task;
    }
}
