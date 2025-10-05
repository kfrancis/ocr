using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Microsoft.Maui.Graphics.Color;
using Image = SixLabors.ImageSharp.Image;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Plugin.Maui.OCR.Sample;

public partial class MainPage
{
    private readonly IOcrService _ocr;
    private bool _isPreviewExpanded;
    private bool _isResultsExpanded;
    private byte[] _originalImageData;
    private GridLength _originalLeftColumnWidth;
    private GridLength _originalRightColumnWidth;
    private byte[] _preprocessedImageData;

    public MainPage(IOcrService feature)
    {
        InitializeComponent();

        _ocr = feature;
    }

    private void ExpandResultsBtn_Clicked(object sender, EventArgs e)
    {
        ToggleResultsPanel();
    }

    private void ExpandPreviewBtn_Clicked(object sender, EventArgs e)
    {
        TogglePreviewPanel();
    }

    private void ResultsPanel_Tapped(object sender, TappedEventArgs e)
    {
        // Only expand if we tap the header or empty space, not on the text content
        var tapPosition = e.GetPosition(ResultsPanel);
        if (tapPosition is { Y: < 50 })
        {
            ToggleResultsPanel();
        }
    }

    private void PreviewPanel_Tapped(object sender, TappedEventArgs e)
    {
        // Only expand if we tap the header or empty space
        var tapPosition = e.GetPosition(PreviewPanel);
        if (tapPosition is { Y: < 50 })
        {
            TogglePreviewPanel();
        }
    }

    private void ToggleResultsPanel()
    {
        if (!_isResultsExpanded && !_isPreviewExpanded)
        {
            // Save original column widths before expanding
            _originalLeftColumnWidth = ContentPanels.ColumnDefinitions[0].Width;
            _originalRightColumnWidth = ContentPanels.ColumnDefinitions[1].Width;
        }

        if (_isResultsExpanded)
        {
            // Restore to original state
            ContentPanels.ColumnDefinitions[0].Width = _originalLeftColumnWidth;
            ContentPanels.ColumnDefinitions[1].Width = _originalRightColumnWidth;
            PreviewPanel.IsVisible = true;
            _isResultsExpanded = false;

            // Update expand button icon
            ((FontImageSource)ExpandResultsBtn.ImageSource).Glyph = "\ue5d0"; // expand icon
        }
        else
        {
            // Expand results panel to full width
            ContentPanels.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            ContentPanels.ColumnDefinitions[1].Width = new GridLength(0);
            PreviewPanel.IsVisible = false;
            _isResultsExpanded = true;

            // If other panel was expanded, collapse it
            if (_isPreviewExpanded)
            {
                _isPreviewExpanded = false;
                ((FontImageSource)ExpandPreviewBtn.ImageSource).Glyph = "\ue5d0"; // expand icon
            }

            // Update expand button icon
            ((FontImageSource)ExpandResultsBtn.ImageSource).Glyph = "\ue5cf"; // collapse icon
        }
    }

    private void TogglePreviewPanel()
    {
        if (!_isResultsExpanded && !_isPreviewExpanded)
        {
            // Save original column widths before expanding
            _originalLeftColumnWidth = ContentPanels.ColumnDefinitions[0].Width;
            _originalRightColumnWidth = ContentPanels.ColumnDefinitions[1].Width;
        }

        if (_isPreviewExpanded)
        {
            // Restore to original state
            ContentPanels.ColumnDefinitions[0].Width = _originalLeftColumnWidth;
            ContentPanels.ColumnDefinitions[1].Width = _originalRightColumnWidth;
            ResultsPanel.IsVisible = true;
            _isPreviewExpanded = false;

            // Update expand button icon
            ((FontImageSource)ExpandPreviewBtn.ImageSource).Glyph = "\ue5d0"; // expand icon
        }
        else
        {
            // Expand preview panel to full width
            ContentPanels.ColumnDefinitions[0].Width = new GridLength(0);
            ContentPanels.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            ResultsPanel.IsVisible = false;
            _isPreviewExpanded = true;

            // If other panel was expanded, collapse it
            if (_isResultsExpanded)
            {
                _isResultsExpanded = false;
                ((FontImageSource)ExpandResultsBtn.ImageSource).Glyph = "\ue5d0"; // expand icon
            }

            // Update expand button icon
            ((FontImageSource)ExpandPreviewBtn.ImageSource).Glyph = "\ue5cf"; // collapse icon
        }
    }

    // Add this to your ClearBtn_Clicked method
    private void ResetPanels()
    {
        // Reset panel expansion states
        if (_isResultsExpanded || _isPreviewExpanded)
        {
            ContentPanels.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            ContentPanels.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            ResultsPanel.IsVisible = true;
            PreviewPanel.IsVisible = true;
            _isResultsExpanded = false;
            _isPreviewExpanded = false;

            // Reset button icons
            ((FontImageSource)ExpandResultsBtn.ImageSource).Glyph = "\ue5d0"; // expand icon
            ((FontImageSource)ExpandPreviewBtn.ImageSource).Glyph = "\ue5d0"; // expand icon
        }
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            await _ocr.InitAsync();
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Error", "Failed to initialize OCR service", "OK");
        }
    }

    private void CameraTabBtn_Clicked(object sender, EventArgs e)
    {
        CameraTabBtn.BackgroundColor = Color.FromArgb("#6750A4");
        CameraTabBtn.TextColor = Colors.White;
        FileTabBtn.BackgroundColor = Color.FromArgb("#E0E0E0");
        FileTabBtn.TextColor = Color.FromArgb("#6750A4");

        CameraOptions.IsVisible = true;
        FileOptions.IsVisible = false;
    }

    private void FileTabBtn_Clicked(object sender, EventArgs e)
    {
        FileTabBtn.BackgroundColor = Color.FromArgb("#6750A4");
        FileTabBtn.TextColor = Colors.White;
        CameraTabBtn.BackgroundColor = Color.FromArgb("#E0E0E0");
        CameraTabBtn.TextColor = Color.FromArgb("#6750A4");

        FileOptions.IsVisible = true;
        CameraOptions.IsVisible = false;
    }

    /// <summary>
    ///     Shows the loading overlay with a custom message
    /// </summary>
    private void ShowLoading(string message = "Processing...")
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProcessingLabel.Text = message;
            LoadingOverlay.IsVisible = true;
        });
    }

    /// <summary>
    ///     Hides the loading overlay
    /// </summary>
    private void HideLoading()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadingOverlay.IsVisible = false;
        });
    }


    private void ClearBtn_Clicked(object sender, EventArgs e)
    {
        ResultLbl.Text = "Waiting for results ...";
        CapturedImage.Source = null;
        NoImagePlaceholder.IsVisible = true;
        _originalImageData = null;
        _preprocessedImageData = null;
        ResetPanels();
    }

    private async void CopyBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ResultLbl.Text != "Waiting for results ...")
            {
                // Remove any HTML tags if present
                var plainText = ResultLbl.Text.Replace("<b>", "").Replace("</b>", "");
                await Clipboard.SetTextAsync(plainText);

                // Optional: Show feedback to user
                await DisplayAlertAsync("Success", "Text copied to clipboard", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Error", "Failed to copy text to clipboard", "OK");
        }
    }

    private async void EnhanceImageBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_originalImageData == null)
            {
                return;
            }

            ShowLoading("Enhancing image...");

            // Show loading indicator or disable button
            EnhanceImageBtn.IsEnabled = false;

            try
            {
                // Try different preprocessing parameters
                _preprocessedImageData = ApplyAdvancedPreprocessing(_originalImageData);

                // Display the enhanced image
                await DisplayImage(_preprocessedImageData);

                // Re-run OCR on the enhanced image
                ShowLoading("Running OCR on enhanced image...");
                var options = new OcrOptions.Builder().SetTryHard(TryHardSwitch.IsToggled).Build();
                var result = await _ocr.RecognizeTextAsync(_preprocessedImageData, options);

                // Update the results
                ResultLbl.Text = result.AllText;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to enhance image: {ex.Message}", "OK");
            }
            finally
            {
                // Re-enable button
                EnhanceImageBtn.IsEnabled = true;
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Error", "Failed to enhance image", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async void OpenFromCameraBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo == null)
                {
                    return;
                }

                var result = await ProcessPhoto(photo);
                ResultLbl.Text = result.AllText;
                NoImagePlaceholder.IsVisible = false;
            }
            else
            {
                await DisplayAlertAsync("Sorry", "Image capture is not supported on this device.", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Error", "Failed to open or process the image.", "OK");
        }
    }

    private async void OpenFromCameraUseEventBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo == null)
                {
                    return;
                }

                _ocr.RecognitionCompleted += OnRecognitionCompleted;
                await StartProcessingPhoto(photo);
            }
            else
            {
                await DisplayAlertAsync("Sorry", "Image capture is not supported on this device.", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Error", "Failed to open or process the image.", "OK");
        }
    }

    private async void OpenFromFileBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            var lsPhoto = await MediaPicker.Default.PickPhotoAsync();

            if (lsPhoto != null)
            {
                var result = await ProcessPhoto(lsPhoto);
                ResultLbl.Text = result.AllText;
                NoImagePlaceholder.IsVisible = false;
            }
        }
        catch (Exception)
        {
            // Show a generic DisplayAlertAsync
            await DisplayAlertAsync("Error", "Failed to open or process the image.", "OK");
        }
    }

    private async void OpenFromFileUseEventBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            var lsPhoto = await MediaPicker.Default.PickPhotoAsync();

            if (lsPhoto == null)
            {
                return;
            }

            _ocr.RecognitionCompleted += OnRecognitionCompleted;
            await StartProcessingPhoto(lsPhoto);
        }
        catch (Exception)
        {
            // Show a generic DisplayAlertAsync
            await DisplayAlertAsync("Error", "Failed to open or process the image.", "OK");
        }
    }

    private void OnRecognitionCompleted(object sender, OcrCompletedEventArgs e)
    {
        // Remove the event handler to avoid multiple subscriptions
        _ocr.RecognitionCompleted -= OnRecognitionCompleted;

        // Update UI on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ResultLbl.Text = e is { IsSuccessful: true, Result: not null }
                ? e.Result.AllText
                : $"Error: {(!string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : "Unknown")}";

            NoImagePlaceholder.IsVisible = false;
        });
    }

    /// <summary>
    ///     Takes a photo and processes it using the OCR service.
    /// </summary>
    /// <param name="photo">The photo to process.</param>
    /// <returns>The OCR result.</returns>
    private async Task<OcrResult> ProcessPhoto(FileResult photo)
    {
        try
        {
            ShowLoading("Reading image...");

            // Open a stream to the photo
            await using var sourceStream = await photo.OpenReadAsync();

            // Create a byte array to hold the image data
            _originalImageData = new byte[sourceStream.Length];

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Read the stream into the byte array
            var dataLength = await sourceStream.ReadAsync(_originalImageData, cancellationTokenSource.Token)
                .ConfigureAwait(false);

            if (dataLength <= 0)
            {
                throw new InvalidOperationException("No image bytes");
            }

            ShowLoading("Preprocessing image...");
            _preprocessedImageData = await PreprocessImageForOcr(_originalImageData);

            await DisplayImage(_preprocessedImageData);

            var options = new OcrOptions.Builder().SetTryHard(TryHardSwitch.IsToggled).Build();

            // Process the image data using the OCR service
            ShowLoading("Extracting text...");
            return await _ocr.RecognizeTextAsync(_preprocessedImageData, options, cancellationTokenSource.Token);
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task DisplayImage(byte[] imageData)
    {
        await Dispatcher.DispatchAsync(() =>
        {
            CapturedImage.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
            NoImagePlaceholder.IsVisible = false;
        });
    }

    private static async Task<byte[]> PreprocessImageForOcr(byte[] imageData)
    {
        await using var ms = new MemoryStream(imageData);
        using var image = await Image.LoadAsync<L8>(ms);

        const int MaxDimension = 1500;
        if (image.Width > MaxDimension || image.Height > MaxDimension)
        {
            var ratio = Math.Min((float)MaxDimension / image.Width, (float)MaxDimension / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // Apply preprocessing steps
        image.Mutate(x => x
                .Contrast(1.2f) // Slight contrast boost
                .GaussianBlur(1.1f) // Slight blur to reduce dot matrix noise
                .BinaryThreshold(0.45f) // Binarize at slightly lower threshold to preserve thin text
        );

        image.Mutate(x => x.Pad(10, 10));

        // Convert back to byte array
        using var resultMs = new MemoryStream();
        await image.SaveAsPngAsync(resultMs);
        return resultMs.ToArray();
    }

    private static byte[] ApplyAdvancedPreprocessing(byte[] imageData)
    {
        // Load image as grayscale
        using var grayscaled = new Mat();
        CvInvoke.Imdecode(imageData, ImreadModes.Grayscale, grayscaled);

        // Ensure it's upright first
        if (grayscaled.Width > grayscaled.Height)
        {
            CvInvoke.Rotate(grayscaled, grayscaled, RotateFlags.Rotate90Clockwise);
        }

        // 1. Apply CLAHE (adaptive histogram equalization)
        using var clahed = new Mat();
        CvInvoke.CLAHE(grayscaled, 2.0, new Size(8, 8), clahed);

        // 2. Gaussian Blur to smooth noise slightly
        CvInvoke.MedianBlur(clahed, clahed, 3);

        // 3. Adaptive Thresholding (better for uneven lighting)
        using var thresholded = new Mat();
        CvInvoke.AdaptiveThreshold(
            clahed, thresholded, 255,
            AdaptiveThresholdType.MeanC,
            ThresholdType.BinaryInv, 15, 10);

        // 4. Morphological Open + Close to remove noise and fill gaps
        using var morphKernel =
            CvInvoke.GetStructuringElement(MorphShapes.Rectangle, new Size(2, 2), new Point(-1, -1));
        CvInvoke.MorphologyEx(thresholded, thresholded, MorphOp.Open, morphKernel, new Point(-1, -1), 1,
            BorderType.Reflect, new MCvScalar());
        CvInvoke.MorphologyEx(thresholded, thresholded, MorphOp.Close, morphKernel, new Point(-1, -1), 1,
            BorderType.Reflect, new MCvScalar());

        // Return as byte array
        return thresholded.ToImage<Gray, byte>().ToJpegData();
    }

    /// <summary>
    ///     Takes a photo and starts processing it using the OCR service with events.
    /// </summary>
    /// <param name="photo">The photo to process.</param>
    private async Task StartProcessingPhoto(FileResult photo)
    {
        // Open a stream to the photo
        await using var sourceStream = await photo.OpenReadAsync();

        // Create a byte array to hold the image data
        _originalImageData = new byte[sourceStream.Length];

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Read the stream into the byte array
        var dataLength = await sourceStream.ReadAsync(_originalImageData, cancellationTokenSource.Token);

        if (dataLength <= 0)
        {
            throw new InvalidOperationException("No image bytes");
        }

        _preprocessedImageData = await PreprocessImageForOcr(_originalImageData);

        await DisplayImage(_preprocessedImageData);

        // Process the image data using the OCR service
        await _ocr.StartRecognizeTextAsync(
            _preprocessedImageData,
            new OcrOptions.Builder().SetTryHard(TryHardSwitch.IsToggled).Build(),
            cancellationTokenSource.Token);
    }

    // Add this method to your MainPage class to fix CS0103
    private Task DisplayAlertAsync(string title, string message, string cancel)
    {
        // If MainPage inherits from ContentPage, you can call DisplayAlert directly.
        // Otherwise, use Application.Current.MainPage.DisplayAlert.
        return Application.Current?.Windows[0].Page?.DisplayAlert(title, message, cancel) ?? Task.CompletedTask;
    }
}
