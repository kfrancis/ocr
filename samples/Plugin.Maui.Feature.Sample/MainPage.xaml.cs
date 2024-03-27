using Plugin.Shared.OCR;

namespace Plugin.Maui.Feature.Sample;

public partial class MainPage : ContentPage
{
    readonly IOcrService _ocr;

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

    private async void OpenFromCameraBtn_Clicked(object sender, EventArgs e)
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                // Open a stream to the photo
                using var sourceStream = await photo.OpenReadAsync();

                // Create a byte array to hold the image data
                var imageData = new byte[sourceStream.Length];

                // Read the stream into the byte array
                await sourceStream.ReadAsync(imageData);

                var result = await _ocr.RecognizeTextAsync(imageData);

                ResultLbl.Text = result.AllText;

                ClearBtn.IsEnabled = true;
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
            // Open a stream to the photo
            using var sourceStream = await photo.OpenReadAsync();

            // Create a byte array to hold the image data
            var imageData = new byte[sourceStream.Length];

            // Read the stream into the byte array
            await sourceStream.ReadAsync(imageData);

            var result = await _ocr.RecognizeTextAsync(imageData);

            ResultLbl.Text = result.AllText;

            ClearBtn.IsEnabled = true;
        }
    }

    private void ClearBtn_Clicked(object sender, EventArgs e)
    {
        ResultLbl.Text = string.Empty;
        ClearBtn.IsEnabled = false;
    }
}
