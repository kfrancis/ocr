using System.Text.RegularExpressions;

namespace Plugin.Maui.OCR;

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
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
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
    [Obsolete("Use ExtractPatterns(string, OcrPatternConfig) instead. This method will be removed in a future version.")]
    public static string? ExtractPattern(string input, OcrPatternConfig? config)
    {
        if (string.IsNullOrEmpty(config?.RegexPattern))
        {
            return null;
        }

        var regex = new Regex(config.RegexPattern);
        var match = regex.Match(input);

        if (match.Success && config.ValidationFunction(match.Value))
        {
            return match.Value;
        }

        return null;
    }
    /// <summary>
    /// Extracts all patterns from the input text using the provided configuration.
    /// </summary>
    /// <param name="input">
    /// The input text to extract the patterns from.
    /// </param>
    /// <param name="config">
    /// The configuration to use for pattern extraction.
    /// </param>
    /// <returns>
    /// All extracted patterns, or an empty list if no pattern was found or all patterns failed validation.
    /// </returns>
    public static List<string> ExtractPatterns(string input, OcrPatternConfig? config)
    {
        List<string> result = [];
        if (string.IsNullOrEmpty(config?.RegexPattern))
        {
            return result;
        }

        var regex = new Regex(config.RegexPattern);
        MatchCollection matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            if (match.Success && config.ValidationFunction(match.Value))
            {
                result.Add(match.Value);
            }

        }


        return result;
    }
}

/// <summary>
/// Provides data for the RecognitionCompleted event.
/// </summary>
public class OcrCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="result">
    /// The result of the OCR operation.
    /// </param>
    /// <param name="errorMessage">
    /// Any error message if the OCR operation failed, or empty string otherwise.
    /// </param> {
    public OcrCompletedEventArgs(OcrResult? result, string? errorMessage = null)
    {
        Result = result;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    /// <summary>
    /// Any error message if the OCR operation failed, or empty string otherwise.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Indicates whether the OCR operation was successful.
    /// </summary>
    public bool IsSuccessful => Result?.Success ?? false;

    /// <summary>
    /// The result of the OCR operation.
    /// </summary>
    public OcrResult? Result { get; }
}

/// <summary>
/// The options for OCR.
/// </summary>
public sealed class OcrOptions
{
    private OcrOptions(string? language, bool tryHard, List<OcrPatternConfig> patternConfigs, CustomOcrValidationCallback? customCallback)
    {
        Language = language;
        TryHard = tryHard;
        PatternConfigs = patternConfigs;
        CustomCallback = customCallback;
    }

    /// <summary>
    /// A callback after recognition is complete
    /// </summary>
    public CustomOcrValidationCallback? CustomCallback { get; }

    /// <summary>
    /// The bcp-47 language code to use for OCR.
    /// </summary>
    public string? Language { get; }

    /// <summary>
    /// The pattern configurations to use for OCR.
    /// </summary>
    public List<OcrPatternConfig> PatternConfigs { get; }

    /// <summary>
    /// Should the platform attempt to be more thorough vs quick?
    /// </summary>
    public bool TryHard { get; }

    /// <summary>
    /// Builder for OcrOptions.
    /// </summary>
    public class Builder
    {
        private CustomOcrValidationCallback? _customCallback;
        private string? _language;
        private List<OcrPatternConfig> _patternConfigs = new();
        private bool _tryHard;

        /// <summary>
        /// Adds an ocr pattern config to the options
        /// </summary>
        /// <param name="patternConfig">
        /// The pattern configuration to add.
        /// </param>
        /// <returns>
        /// The builder.
        /// </returns>
        public Builder AddPatternConfig(OcrPatternConfig patternConfig)
        {
            _patternConfigs.Add(patternConfig);
            return this;
        }

        /// <summary>
        /// Build the OCR options
        /// </summary>
        /// <returns>
        /// The OCR options.
        /// </returns>
        public OcrOptions Build()
        {
            return new OcrOptions(_language, _tryHard, _patternConfigs, _customCallback);
        }

        /// <summary>
        /// Sets a custom callback
        /// </summary>
        /// <param name="customCallback">
        /// A callback after recognition is complete
        /// </param>
        /// <returns>
        /// The builder.
        /// </returns>
        public Builder SetCustomCallback(CustomOcrValidationCallback customCallback)
        {
            _customCallback = customCallback;
            return this;
        }

        /// <summary>
        /// Sets the language
        /// </summary>
        /// <param name="language">
        /// The bcp-47 language code to use for OCR.
        /// </param>
        /// <returns>
        /// The builder.
        /// </returns>
        public Builder SetLanguage(string language)
        {
            _language = language;
            return this;
        }

        /// <summary>
        /// Sets the pattern configurations to use for OCR.
        /// </summary>
        /// <param name="patternConfigs">
        /// The pattern configurations to use for OCR.
        /// </param>
        /// <returns>
        /// The builder.
        /// </returns>
        public Builder SetPatternConfigs(List<OcrPatternConfig>? patternConfigs)
        {
            _patternConfigs = patternConfigs ?? [];
            return this;
        }

        /// <summary>
        /// Sets the accuracy level
        /// </summary>
        /// <param name="tryHard">
        /// Should the platform attempt to be more thorough vs quick?
        /// </param>
        /// <returns>
        /// The builder.
        /// </returns>
        public Builder SetTryHard(bool tryHard)
        {
            _tryHard = tryHard;
            return this;
        }
    }
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
