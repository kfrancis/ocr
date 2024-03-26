using Foundation;
using Plugin.Shared.OCR;
using UIKit;
using Vision;

namespace Plugin.Maui.OCR;

partial class OcrImplementation : IOcrService
{
    private static readonly object _initLock = new();
    private bool _isInitialized = false;

    public Task InitAsync(CancellationToken ct = default)
    {
        lock (_initLock)
        {
            if (_isInitialized) return Task.CompletedTask;
            _isInitialized = true;
        }

        // Perform any necessary initialization here.
        // Example: Loading models, setting up resources, etc.

        return Task.CompletedTask;
    }

    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Init must be called before RecognizeTextAsync.");
        }

        if (ct.IsCancellationRequested)
        {
            throw new OperationCanceledException(ct);
        }

        var tcs = new TaskCompletionSource<OcrResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        ct.Register(() => tcs.TrySetCanceled());

        try
        {
            var image = ImageFromByteArray(imageData) ?? throw new ArgumentException("Invalid image data");

            var recognizeTextRequest = new VNRecognizeTextRequest((request, error) =>
            {
                if (error != null)
                {
                    tcs.TrySetException(new Exception(error.ToString()));
                    return;
                }

                if (ct.IsCancellationRequested)
                {
                    tcs.TrySetCanceled(ct);
                    return;
                }

                var result = ProcessRecognitionResults(request);
                tcs.TrySetResult(result);
            });

            var ocrHandler = new VNImageRequestHandler(image.CGImage, new NSDictionary());
            ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out NSError performError);

            if (performError != null)
            {
                throw new Exception(performError.ToString());
            }
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return await tcs.Task;
    }

    private static OcrResult ProcessRecognitionResults(VNRequest request)
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

                // Splitting by spaces to create elements might not be accurate for all languages/scripts
                topCandidate.String.Split(" ").ToList().ForEach(e =>
                {
                    ocrResult.Elements.Add(new OcrResult.OcrElement { Text = e, Confidence = topCandidate.Confidence });
                });
            }
        }

        ocrResult.Success = true;
        return ocrResult;
    }

    private static UIImage? ImageFromByteArray(byte[] data)
    {
        return data != null ? new UIImage(NSData.FromArray(data)) : null;
    }
}