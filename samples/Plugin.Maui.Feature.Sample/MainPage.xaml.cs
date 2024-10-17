using Plugin.Maui.OCR;

namespace Plugin.Maui.Feature.Sample;

public partial class MainPage : ContentPage
{
    private readonly IOcrService _ocr;

    public MainPage(IOcrService feature)
    {
        InitializeComponent();

        _ocr = feature;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _ocr.InitAsync();
    }

    private void ClearBtn_Clicked(object sender, EventArgs e)
    {
        ResultLbl.Text = string.Empty;
        ClearBtn.IsEnabled = false;
    }

    private async void OpenFromCameraBtn_Clicked(object sender, EventArgs e)
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                var result = await ProcessPhoto(photo);

                ResultLbl.Text = result.AllText;

                ClearBtn.IsEnabled = true;
            }
        }
        else
        {
            await DisplayAlert(title: "Sorry", message: "Image capture is not supported on this device.", cancel: "OK");
        }
    }

    private async void OpenFromCameraUseEventBtn_Clicked(object sender, EventArgs e)
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                _ocr.RecognitionCompleted += (s, e) =>
                {
                    ResultLbl.Text = e.Result.AllText;
                    ClearBtn.IsEnabled = true;
                };
                await StartProcessingPhoto(photo);
            }
        }
        else
        {
            await DisplayAlert(title: "Sorry", message: "Image capture is not supported on this device.", cancel: "OK");
        }
    }

    private async void OpenFromFileBtn_Clicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo != null)
        {
            var result = await ProcessPhoto(photo);

            ResultLbl.Text = result.AllText;

            ClearBtn.IsEnabled = true;
        }
    }

    private async void OpenFromFileUseEventBtn_Clicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.PickPhotoAsync();

        if (photo == null)
        {
            return;
        }

        _ocr.RecognitionCompleted += (s, c) =>
        {
            ResultLbl.Text = c is { IsSuccessful: true, Result: not null } ? c.Result.AllText : $"Error: {c.ErrorMessage}";

            ClearBtn.IsEnabled = true;
        };
        await StartProcessingPhoto(photo);
    }

    /// <summary>
    /// Takes a photo and processes it using the OCR service.
    /// </summary>
    /// <param name="photo">The photo to process.</param>
    /// <returns>The OCR result.</returns>
    private async Task<OcrResult> ProcessPhoto(FileResult photo)
    {
        // Open a stream to the photo
        using var sourceStream = await photo.OpenReadAsync();

        // Create a byte array to hold the image data
        var imageData = new byte[sourceStream.Length];

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Read the stream into the byte array
        await sourceStream.ReadAsync(imageData, cancellationTokenSource.Token);

        var options = new OcrOptions.Builder().SetTryHard(TryHardSwitch.IsToggled).Build();

        // Process the image data using the OCR service
        return await _ocr.RecognizeTextAsync(imageData, options, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Takes a photo and processes it using the OCR service.
    /// </summary>
    /// <param name="photo">The photo to process.</param>
    /// <returns>The OCR result.</returns>
    private async Task StartProcessingPhoto(FileResult photo)
    {
        // Open a stream to the photo
        using var sourceStream = await photo.OpenReadAsync();

        // Create a byte array to hold the image data
        var imageData = new byte[sourceStream.Length];

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Read the stream into the byte array
        await sourceStream.ReadAsync(imageData, cancellationTokenSource.Token);

        // Process the image data using the OCR service
        await _ocr.StartRecognizeTextAsync(imageData, new OcrOptions.Builder().SetTryHard(TryHardSwitch.IsToggled).Build(), cancellationTokenSource.Token);
    }
}
