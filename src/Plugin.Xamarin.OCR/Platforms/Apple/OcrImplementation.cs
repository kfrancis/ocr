using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Vision;
using ImageIO;

#if IOS
using UIKit;
#endif

namespace Plugin.Xamarin.OCR
{
#if !MACOS
    internal class OcrImplementation : IOcrService
    {
        private static readonly object s_initLock = new();
        private bool _isInitialized;
        private IReadOnlyCollection<string> _supportedLanguages;

        public IReadOnlyCollection<string> SupportedLanguages => _supportedLanguages;

        public event EventHandler<OcrCompletedEventArgs> RecognitionCompleted;

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

        private static IEnumerable<string> GetSupportedRecognitionLanguages(VNRecognizeTextRequest recognizeTextRequest, VNRequestTextRecognitionLevel textRecognitionLevel, out NSError? langError)
        {
            int majorVersion;
            var isIOS = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone || UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
            var isMacOS = !isIOS;

            // For iOS, we use UIDevice to check the version
            if (isIOS)
            {
                majorVersion = int.Parse(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
            }
            // For macOS, we use NSProcessInfo
            else
            {
                var version = NSProcessInfo.ProcessInfo.OperatingSystemVersion;
                majorVersion = (int)version.Major;
            }

            if ((isIOS && majorVersion >= 15) || (isMacOS && majorVersion >= 12))
            {
                // New instance method available from iOS 15.0, macOS 12.0
                return recognizeTextRequest.GetSupportedRecognitionLanguages(out langError).Select(ns => ns.ToString());
            }
            else if ((isIOS && majorVersion >= 13) || (isMacOS && majorVersion >= 11))
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

        /// <summary>
        /// Takes an image and returns the text found in the image.
        /// </summary>
        /// <param name="imageData">The image data</param>
        /// <param name="tryHard">True to try and tell the API to be more accurate, otherwise just be fast.</param>
        /// <param name="ct">An optional cancellation token</param>
        /// <returns>The OCR result</returns>
        public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(InitAsync)} must be called before {nameof(RecognizeTextAsync)}.");
            }

            ct.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<OcrResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled());

            VNImageRequestHandler? ocrHandler = null;
            try
            {
                using var srcImage = ImageFromByteArray(imageData) ?? throw new ArgumentException("Invalid image data");
                var imageSize = srcImage.Size;
                CGImagePropertyOrientation? imageOrientation = null;
                switch (srcImage.Orientation)
                {
                    case UIImageOrientation.Up:
                        imageOrientation = CGImagePropertyOrientation.Up;
                        break;
                    case UIImageOrientation.Down:
                        imageOrientation = CGImagePropertyOrientation.Down;
                        break;
                    case UIImageOrientation.Left:
                        imageOrientation = CGImagePropertyOrientation.Left;
                        break;
                    case UIImageOrientation.Right:
                        imageOrientation = CGImagePropertyOrientation.Right;
                        break;
                    case UIImageOrientation.UpMirrored:
                        imageOrientation = CGImagePropertyOrientation.UpMirrored;
                        break;
                    case UIImageOrientation.DownMirrored:
                        imageOrientation = CGImagePropertyOrientation.DownMirrored;
                        break;
                    case UIImageOrientation.LeftMirrored:
                        imageOrientation = CGImagePropertyOrientation.LeftMirrored;
                        break;
                    case UIImageOrientation.RightMirrored:
                        imageOrientation = CGImagePropertyOrientation.RightMirrored;
                        break;
                }

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

                    var result = ProcessOcrResult(request, imageSize);
                    tcs.TrySetResult(result);
                });

                switch (tryHard)
                {
                    case true:
                        recognizeTextRequest.RecognitionLevel = VNRequestTextRecognitionLevel.Accurate;
                        break;

                    case false:
                        recognizeTextRequest.RecognitionLevel = VNRequestTextRecognitionLevel.Fast;
                        break;
                }

                recognizeTextRequest.UsesLanguageCorrection = tryHard;
                recognizeTextRequest.UsesCpuOnly = false;
                recognizeTextRequest.PreferBackgroundProcessing = true;
                recognizeTextRequest.MinimumTextHeight = 0;

                NSError? error = null;

                if (imageOrientation != null)
                {
                    ocrHandler = new VNImageRequestHandler(srcImage.CGImage, orientation: imageOrientation.Value, new NSDictionary());
                    ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out error);
                }
                else
                {
                    ocrHandler = new VNImageRequestHandler(srcImage.CGImage, new NSDictionary());
                    ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out error);
                }

                if (error != null)
                {
                    throw new InvalidOperationException(error.LocalizedDescription);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                ocrHandler?.Dispose();
                ocrHandler = null;
            }

            return await tcs.Task;
        }

        public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(InitAsync)} must be called before {nameof(RecognizeTextAsync)}.");
            }

            ct.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<OcrResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            ct.Register(() => tcs.TrySetCanceled());
            VNImageRequestHandler? ocrHandler = null;
            try
            {
                using var srcImage = ImageFromByteArray(imageData) ?? throw new ArgumentException("Invalid image data");
                var imageSize = srcImage.Size;
                CGImagePropertyOrientation? imageOrientation = null;
                switch (srcImage.Orientation)
                {
                    case UIImageOrientation.Up:
                        imageOrientation = CGImagePropertyOrientation.Up;
                        break;
                    case UIImageOrientation.Down:
                        imageOrientation = CGImagePropertyOrientation.Down;
                        break;
                    case UIImageOrientation.Left:
                        imageOrientation = CGImagePropertyOrientation.Left;
                        break;
                    case UIImageOrientation.Right:
                        imageOrientation = CGImagePropertyOrientation.Right;
                        break;
                    case UIImageOrientation.UpMirrored:
                        imageOrientation = CGImagePropertyOrientation.UpMirrored;
                        break;
                    case UIImageOrientation.DownMirrored:
                        imageOrientation = CGImagePropertyOrientation.DownMirrored;
                        break;
                    case UIImageOrientation.LeftMirrored:
                        imageOrientation = CGImagePropertyOrientation.LeftMirrored;
                        break;
                    case UIImageOrientation.RightMirrored:
                        imageOrientation = CGImagePropertyOrientation.RightMirrored;
                        break;
                }

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

                    var result = ProcessOcrResult(request, imageSize, options);
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

                recognizeTextRequest.UsesLanguageCorrection = options.TryHard;
                recognizeTextRequest.UsesCpuOnly = false;
                recognizeTextRequest.PreferBackgroundProcessing = true;
                recognizeTextRequest.MinimumTextHeight = 0;

                NSError? error = null;

                if (imageOrientation != null)
                {
                    ocrHandler = new VNImageRequestHandler(srcImage.CGImage, orientation: imageOrientation.Value, new NSDictionary());
                    ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out error);
                }
                else
                {
                    ocrHandler = new VNImageRequestHandler(srcImage.CGImage, new NSDictionary());
                    ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out error);
                }

                if (error != null)
                {
                    throw new InvalidOperationException(error.LocalizedDescription);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                ocrHandler?.Dispose();
                ocrHandler = null;
            }

            return await tcs.Task;
        }

        public Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(InitAsync)} must be called before {nameof(StartRecognizeTextAsync)}.");
            }

            ct.ThrowIfCancellationRequested();

            try
            {
                using var image = ImageFromByteArray(imageData) ?? throw new ArgumentException("Invalid image data");
                var imageSize = image.Size;

                using var recognizeTextRequest = new VNRecognizeTextRequest((request, error) =>
                {
                    if (error != null)
                    {
                        RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(null, error.LocalizedDescription));
                        return;
                    }

                    if (ct.IsCancellationRequested)
                    {
                        RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(null, "Operation was cancelled."));
                        return;
                    }

                    try
                    {
                        var result = ProcessOcrResult(request, imageSize, options);
                        RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(result, null));
                    }
                    catch (Exception ex)
                    {
                        RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(null, ex.Message));
                    }
                });

                // Set the recognition level based on options.TryHard
                recognizeTextRequest.RecognitionLevel = options.TryHard ? VNRequestTextRecognitionLevel.Accurate : VNRequestTextRecognitionLevel.Fast;

                // Handle language options
                if (!string.IsNullOrEmpty(options.Language))
                {
                    var supportedLangs = GetSupportedRecognitionLanguages(recognizeTextRequest, recognizeTextRequest.RecognitionLevel, out var langError);
                    if (langError != null)
                    {
                        throw new InvalidOperationException(langError.LocalizedDescription);
                    }
                    var supportedLangList = new List<string>(supportedLangs.Select(ns => ns));

                    if (supportedLangList.Contains(options.Language))
                    {
                        recognizeTextRequest.RecognitionLanguages = new[] { options.Language };
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported language \"{options.Language}\". Supported languages are: {string.Join(", ", supportedLangList)}");
                    }
                }

                // Set other recognition options
                recognizeTextRequest.UsesLanguageCorrection = options.TryHard;
                recognizeTextRequest.UsesCpuOnly = false;
                recognizeTextRequest.PreferBackgroundProcessing = true;
                recognizeTextRequest.MinimumTextHeight = 0;

                using var ocrHandler = new VNImageRequestHandler(image.CGImage, new NSDictionary());
                ocrHandler.Perform(new VNRequest[] { recognizeTextRequest }, out var handlerError);
                if (handlerError != null)
                {
                    RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(null, handlerError.LocalizedDescription));
                }
            }
            catch (Exception ex)
            {
                RecognitionCompleted?.Invoke(this, new OcrCompletedEventArgs(null, ex.Message));
            }

            return Task.CompletedTask;
        }

        private static OcrResult ProcessOcrResult(VNRequest request, CGSize imageSize, OcrOptions? options = null)
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

            if (options?.PatternConfigs != null)
            {
                foreach (var config in options.PatternConfigs)
                {
                    var match = OcrPatternMatcher.ExtractPattern(ocrResult.AllText, config);
                    if (!string.IsNullOrEmpty(match))
                    {
                        ocrResult.MatchedValues.Add(match);
                    }
                }
            }

            options?.CustomCallback?.Invoke(ocrResult.AllText);

            ocrResult.Success = true;

            return ocrResult;
        }

        private static UIImage? ImageFromByteArray(byte[] data)
        {
            return data != null ? new UIImage(NSData.FromArray(data)) : null;
        }
    }
#endif
}
