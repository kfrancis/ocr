using Plugin.Maui.OCR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Microsoft.Maui.Graphics.Color;
using Image = SixLabors.ImageSharp.Image;

namespace Plugin.Maui.Feature.Sample;

public partial class MainPage
{
    private readonly IOcrService _ocr;
    private byte[] _originalImageData;
    private byte[] _preprocessedImageData;
    private bool _isResultsExpanded = false;
    private bool _isPreviewExpanded = false;
    private GridLength _originalLeftColumnWidth;
    private GridLength _originalRightColumnWidth;

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
        if (tapPosition != null && tapPosition.Value.Y < 50)
        {
            ToggleResultsPanel();
        }
    }

    private void PreviewPanel_Tapped(object sender, TappedEventArgs e)
    {
        // Only expand if we tap the header or empty space
        var tapPosition = e.GetPosition(PreviewPanel);
        if (tapPosition != null && tapPosition.Value.Y < 50)
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

            // If other panel was expanded, unexpand it
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

            // If other panel was expanded, unexpand it
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
        base.OnAppearing();

        await _ocr.InitAsync();
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
    /// Shows the loading overlay with a custom message
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
    /// Hides the loading overlay
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
        if (ResultLbl.Text != "Waiting for results ...")
        {
            // Remove any HTML tags if present
            var plainText = ResultLbl.Text.Replace("<b>", "").Replace("</b>", "");
            await Clipboard.SetTextAsync(plainText);

            // Optional: Show feedback to user
            await DisplayAlert("Success", "Text copied to clipboard", "OK");
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
                _preprocessedImageData = await ApplyAdvancedPreprocessing(_originalImageData);

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
                await DisplayAlert("Error", $"Failed to enhance image: {ex.Message}", "OK");
            }
            finally
            {
                // Re-enable button
                EnhanceImageBtn.IsEnabled = true;
            }
        }
        finally
        {
            HideLoading();
        }
    }

    private async void OpenFromCameraBtn_Clicked(object sender, EventArgs e)
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
            await DisplayAlert("Sorry", "Image capture is not supported on this device.", "OK");
        }
    }

    private async void OpenFromCameraUseEventBtn_Clicked(object sender, EventArgs e)
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
            await DisplayAlert("Sorry", "Image capture is not supported on this device.", "OK");
        }
    }

    private async void OpenFromFileBtn_Clicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo != null)
        {
            var result = await ProcessPhoto(photo);
            ResultLbl.Text = result.AllText;
            NoImagePlaceholder.IsVisible = false;
        }
    }

    private async void OpenFromFileUseEventBtn_Clicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo == null)
        {
            return;
        }

        _ocr.RecognitionCompleted += OnRecognitionCompleted;
        await StartProcessingPhoto(photo);
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
                .Contrast(1.2f)         // Slight contrast boost
                .GaussianBlur(1.1f)     // Slight blur to reduce dot matrix noise
                .BinaryThreshold(0.45f)  // Binarize at slightly lower threshold to preserve thin text
        );

        image.Mutate(x => x.Pad(10, 10));

        // Convert back to byte array
        using var resultMs = new MemoryStream();
        await image.SaveAsPngAsync(resultMs);
        return resultMs.ToArray();
    }

    private static async Task<byte[]> ApplyAdvancedPreprocessing(byte[] imageData)
    {
        await using var ms = new MemoryStream(imageData);
        using var image = await Image.LoadAsync<L8>(ms);

        const int MaxDimension = 2000; // Higher resolution for better detail
        if (image.Width > MaxDimension || image.Height > MaxDimension)
        {
            var ratio = Math.Min((float)MaxDimension / image.Width, (float)MaxDimension / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // Apply more advanced preprocessing for difficult images
        image.Mutate(x => x
                .Contrast(1.4f) // Higher contrast
                .MedianBlur(1, true) // Better noise reduction while preserving edges
                .BinaryThreshold(0.55f) // Lower threshold can help with faded text
        );

        // Add a border to help OCR engines recognize boundaries
        image.Mutate(x => x.Pad(20, 20));

        // Convert back to byte array
        using var resultMs = new MemoryStream();
        await image.SaveAsPngAsync(resultMs);
        return resultMs.ToArray();
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
}
