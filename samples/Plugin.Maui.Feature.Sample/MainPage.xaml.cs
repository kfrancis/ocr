using Plugin.Maui.Feature;

namespace Plugin.Maui.Feature.Sample;

public partial class MainPage : ContentPage
{
	readonly IFeature feature;

	public MainPage(IFeature feature)
	{
		InitializeComponent();
		
		this.feature = feature;
	}
}
