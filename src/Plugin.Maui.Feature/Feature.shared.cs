namespace Plugin.Maui.Feature;

public static class Feature
{
	static IFeature? defaultImplementation;

	/// <summary>
	/// Provides the default implementation for static usage of this API.
	/// </summary>
	public static IFeature Default =>
		defaultImplementation ??= new FeatureImplementation();

	internal static void SetDefault(IFeature? implementation) =>
		defaultImplementation = implementation;
}
