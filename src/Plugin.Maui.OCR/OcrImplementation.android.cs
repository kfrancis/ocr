using System.Diagnostics;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.Util;
using Plugin.Shared.OCR;
using Xamarin.Google.MLKit.Common;
using Xamarin.Google.MLKit.Vision.Common;
using Xamarin.Google.MLKit.Vision.Text;
using Xamarin.Google.MLKit.Vision.Text.Latin;
using static Plugin.Shared.OCR.OcrResult;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Maui.OCR;

internal partial class OcrImplementation : IOcrService
{
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
                        Confidence = element.Confidence
                    };
                    ocrResult.Elements.Add(ocrElement);
                }
            }
        }
        ocrResult.Success = true;
        return ocrResult;
    }

    /// <inheritdoc/>
    public Task InitAsync(System.Threading.CancellationToken ct = default)
    {
        // Initialization might not be required for ML Kit's on-device text recognition,
        // but you can perform any necessary setup here.

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, System.Threading.CancellationToken ct = default)
    {
        var image = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
        using var inputImage = InputImage.FromBitmap(image, 0);

        using var textScanner = TextRecognition.GetClient(TextRecognizerOptions.DefaultOptions);

        MlKitException? lastException = null;
        const int MaxRetries = 5;

        for (var retry = 0; retry < MaxRetries; retry++)
        {
            try
            {
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

    private static Task<Java.Lang.Object> ToAwaitableTask(global::Android.Gms.Tasks.Task task)
    {
        var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
        var taskCompleteListener = new TaskCompleteListener(taskCompletionSource);
        task.AddOnCompleteListener(taskCompleteListener);

        return taskCompletionSource.Task;
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
