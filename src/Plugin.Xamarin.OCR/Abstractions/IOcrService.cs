using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Xamarin.OCR;

/// <summary>
/// OCR API.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Initialize the OCR on the platform
    /// </summary>
    /// <param name="ct">An optional cancellation token</param>
    Task InitAsync(CancellationToken ct = default);

    /// <summary>
    /// Takes an image and returns the text found in the image.
    /// </summary>
    /// <param name="imageData">The image data</param>
    /// <param name="ct">An optional cancellation token</param>
    /// <returns>The OCR result</returns>
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default);
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
        /// The text of the element.
        /// </summary>
        public string Text { get; set; }
    }
}
