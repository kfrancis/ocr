using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Java.Lang;
using Java.Util.Concurrent;
using Xamarin.Google.MLKit.Common;
using Xamarin.Google.MLKit.Vision.Common;
using Xamarin.Google.MLKit.Vision.Text;
using Xamarin.Google.MLKit.Vision.Text.Latin;
using Android.Gms.Extensions;
using static Plugin.Maui.OCR.OcrResult;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Maui.OCR;

internal class OcrImplementation : IOcrService, IDisposable
{
    private static readonly IExecutorService? s_executorService = Executors.NewFixedThreadPool(Runtime.GetRuntime()?.AvailableProcessors() ?? 1);
    private static readonly ITextRecognizer? s_textRecognizer = TextRecognition.GetClient(TextRecognizerOptions.DefaultOptions);

    public event EventHandler<OcrCompletedEventArgs>? RecognitionCompleted;

    // Define the supported languages
    private static readonly IReadOnlyCollection<string> s_cloudSupportedLanguages = new List<string>
    {
        // ReSharper disable once StringLiteralTypo
        "ar", "zh-Hans", "zh-Hant", "da", "nl", "en", "fi", "fr", "de", "el", "hi", "hu", "it", "ja", "ko",
        "no", "pl", "pt", "ru", "es", "sv", "th", "tr", "vi"
    };

    // On-device recognizer typically only supports Latin-based scripts
    private static readonly IReadOnlyCollection<string> s_onDeviceSupportedLanguages = new List<string>
    {
        "en", "es", "fr", "de", "it", "pt" // Adjust this list as per the actual support
    };

    // Implement the SupportedLanguages property
    public IReadOnlyCollection<string> SupportedLanguages => s_onDeviceSupportedLanguages;

    // Adjust the property dynamically based on options
    public static IReadOnlyCollection<string> GetSupportedLanguages(bool tryHard) =>
        tryHard ? s_cloudSupportedLanguages : s_onDeviceSupportedLanguages;

    public static OcrResult ProcessOcrResult(Java.Lang.Object result, OcrOptions options)
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
                    if (element.BoundingBox == null)
                        continue;

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

        foreach (var match in from config in options.PatternConfigs
                              let match = OcrPatternMatcher.ExtractPattern(ocrResult.AllText, config)
                              where !string.IsNullOrEmpty(match)
                              select match)
        {
            ocrResult.MatchedValues.Add(match);
        }

        options.CustomCallback?.Invoke(ocrResult.AllText);

        ocrResult.Success = true;
        return ocrResult;
    }

    /// <summary>
    /// Initialize the OCR on the platform
    /// </summary>
    /// <param name="ct">An optional cancellation token</param>
    public Task InitAsync(CancellationToken ct = default)
    {
        // Initialization might not be required for ML Kit's on-device text recognition,
        // but you can perform any necessary setup here.

        return Task.CompletedTask;
    }

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="tryHard">True to try and tell the API to be more accurate, otherwise just be fast.</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default)
    {
        return await RecognizeTextAsync(imageData, new OcrOptions.Builder().SetTryHard(tryHard).Build(), ct);
    }

    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
    {
        using var srcBitmap = await BitmapFactory.DecodeByteArrayAsync(imageData, 0, imageData.Length);
        using var srcImage = InputImage.FromBitmap(srcBitmap, 0);

        MlKitException? lastException = null;
        const int MaxRetries = 5;

        for (var retry = 0; retry < MaxRetries; retry++)
        {
            ITextRecognizer? textScanner = null;

            try
            {
                textScanner = options.TryHard
                    ? TextRecognition.GetClient(new TextRecognizerOptions.Builder()
                        .SetExecutor(s_executorService)
                        .Build())
                    : s_textRecognizer;

                // Try to perform the OCR operation. We should be installing the model necessary when this app is installed, but just in case
                if (textScanner != null)
                {
                    var result = await textScanner.Process(srcImage).AsAsync<Text>();
                    return ProcessOcrResult(result, options);
                }
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
                // Dispose only if we created a temporary cloud recognizer (TryHard case)
                if (textScanner != null && textScanner != s_textRecognizer && options.TryHard)
                    textScanner.Dispose();
            }
        }

        // If all retries have failed, throw the last exception
        if (lastException != null)
        {
            throw lastException;
        }

        throw new InvalidOperationException("OCR operation failed without an exception.");
    }

    public async Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
    {
        using var srcBitmap = await BitmapFactory.DecodeByteArrayAsync(imageData, 0, imageData.Length);
        using var srcImage = InputImage.FromBitmap(srcBitmap, 0);

        MlKitException? lastException = null;
        const int MaxRetries = 5;

        for (var retry = 0; retry < MaxRetries; retry++)
        {
            ITextRecognizer? textScanner = null;

            try
            {
                textScanner = options.TryHard
                    ? TextRecognition.GetClient(new TextRecognizerOptions.Builder()
                        .SetExecutor(s_executorService)
                        .Build())
                    : s_textRecognizer;

                if (textScanner != null)
                {
                    var result = ProcessOcrResult(await textScanner.Process(srcImage).AsAsync<Text>(), options);
                    RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(result));
                }

                break;
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
                if (textScanner != null && textScanner != s_textRecognizer && options.TryHard)
                    textScanner.Dispose();
            }
        }

        // If all retries have failed, throw the last exception
        if (lastException != null)
            RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(null, lastException.Message));
    }

    public void Dispose()
    {
        // No disposal of static singletons here (avoids S2696 pattern and cross-instance issues)
        GC.SuppressFinalize(this);
    }
}
