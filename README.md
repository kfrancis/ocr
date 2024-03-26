<!-- 
Everything in here is of course optional. If you want to add/remove something, absolutely do so as you see fit.
This example README has some dummy APIs you'll need to replace and only acts as a placeholder for some inspiration that you can fill in with your own functionalities.
-->
![](nuget.png)
# Plugin.Xamarin.OCR | Plugin.Maui.OCR

`Plugin.Xamarin.OCR` and `Plugin.Maui.OCR` provides the ability to do simple text from image OCR using nothing but platform APIs.

## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.OCR.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.OCR/)

Available on NuGet for [MAUI](http://www.nuget.org/packages/Plugin.Maui.OCR) and [Xamarin](http://www.nuget.org/packages/Plugin.Xamarin.OCR).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.OCR` or `dotnet add package Plugin.Xamarin.OCR`, or through the NuGet Package Manager in Visual Studio.

### Supported Platforms

| Platform | Minimum Version Supported |
|----------|---------------------------|
| iOS      | 11+                       |
| macOS    | 10.15+                    |
| Android  | 5.0 (API 21)              |
| Windows  | 11 and 10 version 1809+   |

## API Usage

For MAUI, to initialize make sure you use `AddOcr()` like so:

```csharp
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).
			AddOcr();  // <-- add this line

		return builder.Build();
	}
}
```

And then you can just inject `IOcrService` into your classes and use it like so:

```csharp

```

The `IOcrService` interface exposes the following methods:

```csharp
public interface IOcrService
{
    Task InitAsync(CancellationToken ct = default);
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken ct = default);
}

public class OcrResult
{
    public bool Success { get; set; }

    public string AllText { get; set; }

    public IList<OcrElement> Elements { get; set; } = new List<OcrElement>();
    public IList<string> Lines { get; set; } = new List<string>();

    public class OcrElement
    {
        public string Text { get; set; }
        public float Confidence { get; set; }
    }
}
```

### Permissions

Before you can start using Feature, you will need to request the proper permissions on each platform.

#### iOS

No permissions are needed for iOS.

#### Android

No permissions are needed for Android.

### Dependency Injection

You will first need to register the `Feature` with the `MauiAppBuilder` following the same pattern that the .NET MAUI Essentials libraries follow.

```csharp
builder.Services.AddSingleton(Feature.Default);
```

You can then enable your classes to depend on `IFeature` as per the following example.

```csharp
public class FeatureViewModel
{
    readonly IFeature feature;

    public FeatureViewModel(IFeature feature)
    {
        this.feature = feature;
    }

    public void StartFeature()
    {
        feature.ReadingChanged += (sender, reading) =>
        {
            Console.WriteLine(reading.Thing);
        };

        feature.Start();
    }
}
```

### Straight usage

Alternatively if you want to skip using the dependency injection approach you can use the `Feature.Default` property.

```csharp
public class FeatureViewModel
{
    public void StartFeature()
    {
        feature.ReadingChanged += (sender, reading) =>
        {
            Console.WriteLine(feature.Thing);
        };

        Feature.Default.Start();
    }
}
```

### Feature

Once you have created a `Feature` you can interact with it in the following ways:

#### Events

##### `ReadingChanged`

Occurs when feature reading changes.

#### Properties

##### `IsSupported`

Gets a value indicating whether reading the feature is supported on this device.

##### `IsMonitoring`

Gets a value indicating whether the feature is actively being monitored.

#### Methods

##### `Start()`

Start monitoring for changes to the feature.

##### `Stop()`

Stop monitoring for changes to the feature.

# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3
