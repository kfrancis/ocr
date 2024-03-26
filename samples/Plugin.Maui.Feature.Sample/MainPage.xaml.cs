using Plugin.Shared.OCR;

namespace Plugin.Maui.Feature.Sample;

public partial class MainPage : ContentPage
{
    readonly IOcrService _feature;

    public MainPage(IOcrService feature)
    {
        InitializeComponent();

        _feature = feature;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _feature.InitAsync();
    }

    private async void RunOcrBtn_Clicked(object sender, EventArgs e)
    {
        // load test.jpg and then run OCR on it
        using var fileStream = await FileSystem.Current.OpenAppPackageFileAsync("test.jpg");

        // read all bytes of the image from fileStream
        var imageData = new byte[fileStream.Length];
        await fileStream.ReadAsync(imageData, 0, imageData.Length);

        var result = await _feature.RecognizeTextAsync(imageData);
    }
}
