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
}
