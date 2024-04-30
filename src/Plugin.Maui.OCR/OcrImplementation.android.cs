using System.Diagnostics;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.Util;
using Java.Util.Concurrent;
using Xamarin.Google.MLKit.Common;
using Xamarin.Google.MLKit.Vision.Common;
using Xamarin.Google.MLKit.Vision.Text;
using Xamarin.Google.MLKit.Vision.Text.Latin;
using static Plugin.Maui.OCR.OcrResult;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Maui.OCR;

class OcrImplementation : IOcrService
{
    public IReadOnlyCollection<string> SupportedLanguages => throw new NotImplementedException();

    public static OcrResult ProcessOcrResult(Java.Lang.Object result)
    {
        var ocrResult = new OcrResult();
        var textResult = (Text)result;

        ocrResult.AllText = textResult.GetText();
        foreach (var block in textResult.TextBlocks)
        {
            foreach (var line in block.Lines)
            {
                ocrResult.Lines.Add(line.Text);
                foreach (var element in line.Elements)
                {
                    var ocrElement = new OcrElement
                    {
                        Text = element.Text,
                        Confidence = element.Confidence,
                        X = element.BoundingBox.Left,
                        Y = element.BoundingBox.Top,
                        Width = element.BoundingBox.Width(),
                        Height = element.BoundingBox.Height()
                    };
                    ocrResult.Elements.Add(ocrElement);
                }
            }
        }
        ocrResult.Success = true;
        return ocrResult;
    }

    /// <summary>
    /// Initialize the OCR on the platform
    /// </summary>
    /// <param name="ct">An optional cancellation token</param>
    public Task InitAsync(System.Threading.CancellationToken ct = default)
    {
        // Initialization might not be required for ML Kit's on-device text recognition,
        // but you can perform any necessary setup here.

        return Task.CompletedTask;
    }

    private static Task<Java.Lang.Object> ToAwaitableTask(global::Android.Gms.Tasks.Task task)
    {
        var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
        var taskCompleteListener = new TaskCompleteListener(taskCompletionSource);
        task.AddOnCompleteListener(taskCompleteListener);

        return taskCompletionSource.Task;
    }

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="tryHard">True to try and tell the API to be more accurate, otherwise just be fast.</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, System.Threading.CancellationToken ct = default)
    {
        return await RecognizeTextAsync(imageData, new OcrOptions(TryHard: tryHard), ct);
    }

    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, System.Threading.CancellationToken ct = default)
    {
        var image = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
        using var inputImage = InputImage.FromBitmap(image, 0);

        MlKitException? lastException = null;
        const int MaxRetries = 5;

        for (var retry = 0; retry < MaxRetries; retry++)
        {
            ITextRecognizer? textScanner = null;

            try
            {
                if (options.TryHard)
                {
                    // For more accurate results, use the cloud-based recognizer (requires internet).
                    textScanner = TextRecognition.GetClient(new TextRecognizerOptions.Builder()
                        .SetExecutor(Executors.NewFixedThreadPool(1))
                        .Build());
                }
                else
                {
                    // Use the default on-device recognizer for faster results.
                    textScanner = TextRecognition.GetClient(TextRecognizerOptions.DefaultOptions);
                }

                // Try to perform the OCR operation. We should be installing the model necessary when this app is installed, but just in case ..
                return ProcessOcrResult(await ToAwaitableTask(textScanner.Process(inputImage).AddOnSuccessListener(new OnSuccessListener()).AddOnFailureListener(new OnFailureListener())));
            }
            catch (MlKitException ex) when ((ex.Message ?? string.Empty).Contains("Waiting for the text optional module to be downloaded"))
            {
                // If the specific exception is caught, log it and wait before retrying
                lastException = ex;
                Debug.WriteLine($"OCR model is not ready. Waiting before retrying... Attempt {retry + 1}/{MaxRetries}");
                await Task.Delay(5000, ct);
            }
            finally
            {
                textScanner?.Dispose();
                textScanner = null;
            }
        }

        // If all retries have failed, throw the last exception
        if (lastException != null)
        {
            throw lastException;
        }
        else
        {
            throw new InvalidOperationException("OCR operation failed without an exception.");
        }
    }

    public class OnFailureListener : Java.Lang.Object, IOnFailureListener
    {
        public void OnFailure(Java.Lang.Exception e)
        {
            Log.Debug(nameof(OcrImplementation), e.ToString());
        }
    }

    public class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
    {
        public void OnSuccess(Java.Lang.Object result)
        {
        }
    }

    internal class TaskCompleteListener(TaskCompletionSource<Java.Lang.Object> tcs) : Java.Lang.Object, IOnCompleteListener
    {
        private readonly TaskCompletionSource<Java.Lang.Object> _taskCompletionSource = tcs;

        public void OnComplete(global::Android.Gms.Tasks.Task task)
        {
            if (task.IsCanceled)
            {
                _taskCompletionSource.SetCanceled();
            }
            else if (task.IsSuccessful)
            {
                _taskCompletionSource.SetResult(task.Result);
            }
            else
            {
                _taskCompletionSource.SetException(task.Exception);
            }
        }
    }
}
