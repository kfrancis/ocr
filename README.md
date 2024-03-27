<!-- 
Everything in here is of course optional. If you want to add/remove something, absolutely do so as you see fit.
This example README has some dummy APIs you'll need to replace and only acts as a placeholder for some inspiration that you can fill in with your own functionalities.
-->
![](nuget.png)
# Plugin.Xamarin.OCR | Plugin.Maui.OCR

`Plugin.Xamarin.OCR` and `Plugin.Maui.OCR` provides the ability to do simple text from image OCR using nothing but platform APIs.

## Should you use this yet?

**NO**. This is a work in progress and is not ready for production use. It is not feature complete and is not yet stable.

## What Works Matrix

| Platform | iOS | Android | Windows | macOS |
|----------|-----|---------|---------|-------|
| Xamarin  | WIP | WIP | WIP | WIP |
| MAUI     | Yes | Yes | Yes | Yes |

## Why

Why am I making this? I'm making this because I want to make it easier for developers to do OCR in their apps. I want to make it so that you can just use this plugin and not have to worry about the platform specifics.

To many times I've tried to do OCR and had to wrestle with external dependencies like Tesseract (with it's dependencies Leptonica, etc) and these types
of native dependencies can be a real pain to work with. 

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

You will first need to register the `OCR` with the `MauiAppBuilder` following the same pattern that the .NET MAUI Essentials libraries follow.

```csharp
builder.Services.AddSingleton(OCR.Default);
```

You can then enable your classes to depend on `IFeature` as per the following example.

```csharp
public class OcrViewModel
{
    readonly IOcrService ocr;

    public FeatureViewModel(IOcrService ocr)
    {
        this.ocr = ocr;
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

        OCR.Default.Start();
    }
}
```

### Feature

Once you have created a `Feature` you can interact with it in the following ways:

#### Events



#### Properties



#### Methods

##### `InitAsync()`

Initialize the feature.

##### `RecognizeTextAsync(byte[] imageData)`

Recognize text from an image.

# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3
