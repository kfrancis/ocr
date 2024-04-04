<!-- 
Everything in here is of course optional. If you want to add/remove something, absolutely do so as you see fit.
This example README has some dummy APIs you'll need to replace and only acts as a placeholder for some inspiration that you can fill in with your own functionalities.
-->
![](nuget.png)
# Plugin.Xamarin.OCR | Plugin.Maui.OCR

`Plugin.Xamarin.OCR` and `Plugin.Maui.OCR` provide the ability to do simple text from image OCR using nothing but platform APIs.

## Should you use this yet?

**YES**. Let me know how it works for you.

## What Works Matrix

| Platform | iOS | Android | Windows | macOS |
|----------|-----|---------|---------|-------|
| Xamarin  | Yes |   Yes   |   WIP   |  WIP  |
| MAUI     | Yes |   Yes   |   Yes   |  Yes  |

[![Build for CI](https://github.com/kfrancis/ocr/actions/workflows/ci.yml/badge.svg)](https://github.com/kfrancis/ocr/actions/workflows/ci.yml)

## Why

Why am I making this? I'm doing this because I want to make it easier for developers to do OCR in their apps. I want to make it so that you can just use this plugin and not have to worry about the platform specifics.

Too many times I've tried to do OCR and had to wrestle with external dependencies like Tesseract (with its dependencies Leptonica, etc) and these types
of native dependencies can be a real pain to work with. 

### Xamarin??

Well, I still have to maintain a Xamarin app that uses Tesseract and I'm tired of all the problems that come with it. I want to make it easier for myself and others to do OCR in their apps.

## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.OCR.svg?label=MAUI)](https://www.nuget.org/packages/Plugin.Maui.OCR/)
[![NuGet](https://img.shields.io/nuget/v/Plugin.Xamarin.OCR.svg?label=Xamarin)](https://www.nuget.org/packages/Plugin.Xamarin.OCR/)

Available on NuGet for [MAUI](http://www.nuget.org/packages/Plugin.Maui.OCR) and [Xamarin](http://www.nuget.org/packages/Plugin.Xamarin.OCR).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.OCR` or `dotnet add package Plugin.Xamarin.OCR`, or through the NuGet Package Manager in Visual Studio.

### Supported Platforms

| Platform | Minimum Version Supported |
|----------|---------------------------|
| iOS      | 11+                       |
| macOS    | 10.15+                    |
| Android  | 5.0 (API 21)              |
| Windows  | 11 and 10 version 1809+   |

## MAUI Setup and Usage

For MAUI, to initialize make sure you use the MauiAppBuilder extension `AddOcr()` like so:

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

    // Read the stream into the byte array
    await sourceStream.ReadAsync(imageData);

    // Process the image data using the OCR service
    return await _ocr.RecognizeTextAsync(imageData);
}
```

## Xamarin Setup and Usage

WIP

## Details

The `IOcrService` interface exposes the following methods:

```csharp
public interface IOcrService
{
    Task InitAsync(CancellationToken ct = default);
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default);
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

        // Useful for bounding boxes
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
```

### Permissions

Before you can start using Feature, you will need to request the proper permissions on each platform.

#### iOS

If you're handling camera, you'll need the usual permissions for that.

#### Android

If you're handling camera, you'll need the usual permissions for that. The only extra part you'll want in the AndroidManifest.xml is the following:

```xml
<application ..>
  <meta-data android:name="com.google.mlkit.vision.DEPENDENCIES" android:value="ocr" />
</application>
```

This will cause the model necessary to be installed when the application is installed.

### Dependency Injection

You will first need to register the `OcrPlugin` with the `MauiAppBuilder` following the same pattern that the .NET MAUI Essentials libraries follow.

```csharp
builder.Services.AddSingleton(OcrPlugin.Default);
```

You can then enable your classes to depend on `IOcrService` as per the following example.

```csharp
public class OcrViewModel
{
    readonly IOcrService _ocr;

    public OcrViewModel(IOcrService ocr)
    {
        _ocr = ocr;
    }

    public void DoSomeOcr()
    {
        byte[] imageData = GetImageData();

        var result = await _ocr.RecognizeTextAsync(imageData);
    }
}
```

### Straight usage

Alternatively if you want to skip using the dependency injection approach you can use the `Feature.Default` property.

```csharp
public class OcrViewModel
{
    public void DoSomeOcr()
    {
        byte[] imageData = GetImageData();

        var result = await OcrPlugin.Default.RecognizeTextAsync(imageData);
    }
}
```

### Feature

Once you have the `OCR` instance, you can interact with it in the following ways:

#### Events



#### Properties



#### Methods

##### `InitAsync(CancellationToken ct = default)`

Initialize the feature. Might get removed, most platforms (if not all) don't currently require any addition initialization.

##### `RecognizeTextAsync(byte[] imageData, bool tryHard = false, CancellationToken ct = default)`

Recognize text from an image. Specify "tryHard" if you want to tell the platform API to do a better job (fast vs accurate, and use language correction (ios/mac)) though it seems very accurate normally.

# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3
