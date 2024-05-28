using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Xamarin.OCR;

/// <summary>
/// A callback that can be used to provide custom validation for the extracted text.
/// </summary>
/// <param name="extractedText">
/// The extracted text to validate.
/// </param>
/// <returns>
/// True if the text is valid, otherwise false.
/// </returns>
public delegate bool CustomOcrValidationCallback(string extractedText);

/// <summary>
/// OCR API.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Event triggered when OCR recognition is completed.
    /// </summary>
    event EventHandler<OcrCompletedEventArgs> RecognitionCompleted;

    /// <summary>
    /// BCP-47 language codes supported by the OCR service.
    /// </summary>
    IReadOnlyCollection<string> SupportedLanguages { get; }

    /// <summary>
    /// Initialize the OCR on the platform
    /// </summary>
    /// <param name="ct">An optional cancellation token</param>
    Task InitAsync(CancellationToken ct = default);

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="tryHard">True to try and tell the API to be more accurate, otherwise just be fast.</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default);

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="options">The options for OCR</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default);

    /// <summary>
    /// Takes an image, starts the OCR process and triggers the RecognitionCompleted event when completed.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="options">The options for OCR</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    Task StartRecognizeTextAsync(byte[] imageData, OcrOptions options, CancellationToken ct = default);
}

/// <summary>
/// A helper class to extract patterns from text and use the custom validation function (if defined).
/// </summary>
public static class OcrPatternMatcher
{
    /// <summary>
    /// Extracts a pattern from the input text using the provided configuration.
    /// </summary>
    /// <param name="input">
    /// The input text to extract the pattern from.
    /// </param>
    /// <param name="config">
    /// The configuration to use for pattern extraction.
    /// </param>
    /// <returns>
    /// The extracted pattern, or null if no pattern was found or the pattern failed validation.
    /// </returns>
    public static string? ExtractPattern(string input, OcrPatternConfig config)
    {
        var regex = new Regex(config.RegexPattern);
        var match = regex.Match(input);

        if (match.Success && (config.ValidationFunction == null || config.ValidationFunction(match.Value)))
        {
            return match.Value;
        }
        return null;
    }
}

/// <summary>
/// Provides data for the RecognitionCompleted event.
/// </summary>
public class OcrCompletedEventArgs(OcrResult? result, string? errorMessage = null) : EventArgs
{
    /// <summary>
    /// Any error message if the OCR operation failed, or empty string otherwise.
    /// </summary>
    public string ErrorMessage { get; } = errorMessage ?? string.Empty;

    /// <summary>
    /// Indicates whether the OCR operation was successful.
    /// </summary>
    public bool IsSuccessful => Result?.Success ?? false;

    /// <summary>
    /// The result of the OCR operation.
    /// </summary>
    public OcrResult? Result { get; } = result;
}

/// <summary>
/// The options for OCR.
/// </summary>
public class OcrOptions
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="language">
    /// (Optional) The BCP-47 language code of the language to recognize.
    /// </param>
    /// <param name="tryHard">
    /// (Optional) True to try and tell the API to be more accurate, otherwise just be fast. Default is just to be fast.
    /// </param>
    /// <param name="patternConfigs">
    /// (Optional) The pattern configurations for OCR.
    /// </param>
    /// <param name="customCallback">
    /// (Optional) A callback that can be used to provide custom validation for the extracted text.
    /// </param>
    public OcrOptions(string? language = null, bool tryHard = false, List<OcrPatternConfig>? patternConfigs = null, CustomOcrValidationCallback? customCallback = null)
    {
        Language = language;
        TryHard = tryHard;
        PatternConfigs = patternConfigs;
        CustomCallback = customCallback;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="language">
    /// (Optional) The BCP-47 language code of the language to recognize.
    /// </param>
    /// <param name="tryHard">
    /// (Optional) True to try and tell the API to be more accurate, otherwise just be fast. Default is just to be fast.
    /// </param>
    /// <param name="patternConfig">
    /// (Optional) The pattern configuration for OCR.
    /// </param>
    /// <param name="customCallback">
    /// (Optional) A callback that can be used to provide custom validation for the extracted text.
    /// </param>
    public OcrOptions(string? language = null, bool tryHard = false, OcrPatternConfig? patternConfig = null, CustomOcrValidationCallback? customCallback = null)
    {
        Language = language;
        TryHard = tryHard;
        PatternConfigs = new List<OcrPatternConfig> { patternConfig };
        CustomCallback = customCallback;
    }

    /// <summary>
    /// A callback that can be used to provide custom validation for the extracted text.
    /// </summary>
    public CustomOcrValidationCallback? CustomCallback { get; set; }

    /// <summary>
    /// The BCP-47 language code of the language to recognize.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The pattern configurations for OCR.
    /// </summary>
    public List<OcrPatternConfig>? PatternConfigs { get; set; }

    /// <summary>
    /// True to try and tell the API to be more accurate, otherwise just be fast.
    /// </summary>
    public bool TryHard { get; set; }
}

/// <summary>
/// Configuration for OCR patterns.
/// </summary>
public class OcrPatternConfig
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="regexPattern">
    /// The regex pattern to match.
    /// </param>
    /// <param name="validationFunction">
    /// If provided, the extracted text will be validated against this function.
    /// </param>
    public OcrPatternConfig(string regexPattern, Func<string, bool> validationFunction = null)
    {
        RegexPattern = regexPattern;
        ValidationFunction = validationFunction;
    }

    /// <summary>
    /// The regex pattern to match.
    /// </summary>
    public string RegexPattern { get; set; }

    /// <summary>
    /// If provided, the extracted text will be validated against this function.
    /// </summary>
    public Func<string, bool> ValidationFunction { get; set; }
}

/// <summary>
/// The result of an OCR operation.
/// </summary>
public class OcrResult
{
    /// <summary>
    /// The full text of the OCR result.
    /// </summary>
    public string AllText { get; set; }

    /// <summary>
    /// The individual elements of the OCR result.
    /// </summary>
    public IList<OcrElement> Elements { get; set; } = new List<OcrElement>();

    /// <summary>
    /// The lines of the OCR result.
    /// </summary>
    public IList<string> Lines { get; set; } = new List<string>();

    /// <summary>
    /// The matched values of the OCR result.
    /// </summary>
    public IList<string> MatchedValues { get; set; } = new List<string>();

    /// <summary>
    /// Was the OCR successful?
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The words of the OCR result.
    /// </summary>
    public class OcrElement
    {
        /// <summary>
        /// The confidence of the OCR result.
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// The height of the element.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The text of the element.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The width of the element.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The X coordinates of the element.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y coordinates of the element.
        /// </summary>
        public int Y { get; set; }
    }
}
